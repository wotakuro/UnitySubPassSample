# UnitySubPassSample

ScriptableRenderContext.BeginSubPassを使ったサンプルになります。<br />
タイルベースGPUでは UNITY_READ_FRAMEBUFFER_INPUTのアクセスによって、ColorやDepthへのアクセスが低コストになります

This is an sample project using "ScriptableRenderContext.BeginSubPass".
The performance will be increased accessing via "UNITY_READ_FRAMEBUFFER_INPUT" on the tile based GPU.
