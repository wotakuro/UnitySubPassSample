# UnitySubPassSample

ScriptableRenderContext.BeginSubPassを使ったサンプルになります。<br />
タイルベースGPUでは UNITY_READ_FRAMEBUFFER_INPUTのアクセスによって、ColorやDepthへのアクセスが低コストになります

既知の問題：カスタムレンダーパイプラインが、シーンビューでの描画に対応していません。


This is an sample project using "ScriptableRenderContext.BeginSubPass".
The performance will be increased accessing via "UNITY_READ_FRAMEBUFFER_INPUT" on the tile based GPU.

KnownIssue:SceneView doesn't work well because of Custom RenderPipeline.
