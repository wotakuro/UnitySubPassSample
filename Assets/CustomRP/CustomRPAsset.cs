using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRPAsset : RenderPipelineAsset
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/CreateCustomRPAsset")]
    public static void Create()
    {
        CustomRPAsset asset = CustomRPAsset.CreateInstance< CustomRPAsset>();
        UnityEditor.AssetDatabase.CreateAsset(asset,"Assets/CustomRp.asset");

    }
#endif

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRPInstance();
    }
}

public class CustomRPInstance : RenderPipeline
{
    private RenderTexture colorRt;
    private RenderTexture color2Rt;
    private RenderTexture finalRt;
    private RenderTexture depthRt;
    static Mesh s_FullscreenMesh = null;
    public static Mesh fullscreenMesh
    {
        get
        {
            if (s_FullscreenMesh != null)
                return s_FullscreenMesh;

            float topV = 1.0f;
            float bottomV = 0.0f;

            Mesh mesh = new Mesh { name = "Fullscreen Quad" };
            mesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

            mesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

            mesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
            mesh.UploadMeshData(true);
            return mesh;
        }
    }

    private void InitRenderTextures()
    {
        if (!colorRt)
        {
            colorRt = new RenderTexture(512, 512, 0);
            //colorRt.memorylessMode = RenderTextureMemoryless.Color;
            colorRt.name = "ColorRTTT";
            colorRt.Create();
        }
        if (!color2Rt)
        {
            color2Rt = new RenderTexture(512, 512, 0);
            //color2Rt.memorylessMode = RenderTextureMemoryless.Color;
            color2Rt.name = "Color2RTTT";
            color2Rt.Create();
        }
        if (!depthRt)
        {
            depthRt = new RenderTexture(512, 512, 24);
            depthRt.format = RenderTextureFormat.Depth;
            //depthRt.memorylessMode = RenderTextureMemoryless.Depth;
            depthRt.Create();
        }
        if (!finalRt)
        {
            finalRt = new RenderTexture(512, 512, 0);
            finalRt.Create();
        }
    }
    // どういうバッファーがあるのか定義します
    AttachmentDescriptor color = new AttachmentDescriptor(RenderTextureFormat.ARGB32);
    AttachmentDescriptor color2 = new AttachmentDescriptor(RenderTextureFormat.ARGB32);
    AttachmentDescriptor depth = new AttachmentDescriptor(RenderTextureFormat.Depth);
    AttachmentDescriptor final = new AttachmentDescriptor(RenderTextureFormat.ARGB32);

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        InitRenderTextures();

        color.ConfigureTarget(new RenderTargetIdentifier(colorRt), false, true);
        color2.ConfigureTarget(new RenderTargetIdentifier(color2Rt), false, true);
        depth.ConfigureTarget(new RenderTargetIdentifier(depthRt), false, true);
        final.ConfigureTarget(new RenderTargetIdentifier(finalRt), false, true);

        depth.ConfigureClear(new Color(), 1.0f, 0);

        NativeArray<AttachmentDescriptor> descriptors =
            new NativeArray<AttachmentDescriptor>(new[] {
                color,depth,color2,
                final
            },
            Allocator.Temp);
        foreach (var camera in cameras)
        {
            if (!camera.enabled)
                continue;

            int depthIndex = 1;
            int samples = 1;
            context.BeginRenderPass(finalRt.width, finalRt.height,
                samples, descriptors, depthIndex);
            {
                SubPassStep01(ref context, camera);
                SubPassStep02(ref context);
            }
            context.EndRenderPass();
            cmd.Clear();
            cmd.Blit(color2Rt, new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));
            context.ExecuteCommandBuffer(cmd);
            context.Submit();
        }
        descriptors.Dispose();
    }

    CommandBuffer cmd = new CommandBuffer();
    // 最初のパス
    private void SubPassStep01(ref ScriptableRenderContext context, Camera camera)
    {
        // Clearして、CameraのMatrixセットも行います
        cmd.Clear();
        cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        cmd.ClearRenderTarget(true, true, camera.backgroundColor);
        // 描画設定周り
        ScriptableCullingParameters cullingParams;
        camera.TryGetCullingParameters(out cullingParams);

        var cullResults = context.Cull(ref cullingParams);
        var sortingSettings = new SortingSettings(camera);
        var settings = new DrawingSettings(new ShaderTagId("ForwardBase"), sortingSettings);


        // Descriptorで設定したうちのどのバッファーを利用するか？
        var index = new NativeArray<int>(new[] { 0,2 }, Allocator.Temp);
        context.BeginSubPass(index);
        {
            context.ExecuteCommandBuffer(cmd);
            var fs = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(cullResults, ref settings, ref fs);
        }
        context.EndSubPass();
        index.Dispose();
    }

    // Step01で描画した ColorとDepthを使って最終的な画面にかきだします
    private void SubPassStep02(ref ScriptableRenderContext context)
    {
        cmd.Clear();

        Material material = Resources.Load<Material>("CustomBlit");
        //cmd.ClearRenderTarget(false, true, Color.black);
        // Descripterで設定された 0番目(color)と1番目(depth)を受け取って…
        var inputIndex = new NativeArray<int>(new[] { 0 ,1}, Allocator.Temp);
        // Descripterで設定された 2番目(final)に書きます
        var index = new NativeArray<int>(new[] { 2 }, Allocator.Temp);
        context.BeginSubPass(index, inputIndex, true);
        {
            cmd.ClearRenderTarget(true, true, Color.white);
            // CustomBlit.shaderで全画面を描画します
            cmd.DrawMesh(fullscreenMesh, Matrix4x4.identity, material, 0, 0);
            //cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4);
            context.ExecuteCommandBuffer(cmd);
        }
        context.EndSubPass();

        inputIndex.Dispose();
        index.Dispose();
    }
}