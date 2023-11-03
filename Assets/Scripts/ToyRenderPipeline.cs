using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ToySRP
{
    public sealed class ToyRenderPipeline : RenderPipeline
    {
        readonly ToyRenderPipelineAsset asset;
        readonly CubemapCameraHolder cubemapCameraHolder;

        int renderTarget = Shader.PropertyToID("ToyTarget");
        const string PipelineName = "ToyRenderPipeline";
        const string ToyCubemap = "ToyCubemap";

        public ToyRenderPipeline(ToyRenderPipelineAsset asset, CubemapCameraHolder cubemapCameraHolder)
        {
            this.asset = asset;
            this.cubemapCameraHolder = cubemapCameraHolder;
        }

        // 毎フレーム呼ばれる
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            // depthバッファをクリアする。

            if (cubemapCameraHolder != null)
            {
                RenderCubemap(context, cubemapCameraHolder.Camera);
            }

            foreach (var camera in cameras)
            {
                Render(context, camera);
            }
        }

        void Render(ScriptableRenderContext context, Camera camera)
        {
            var cmd = CommandBufferPool.Get(PipelineName);

            // カメラのカリングパラメータを取得する。
            if (!camera.TryGetCullingParameters(out var cullingParameters)) return;
            // カリングを実行。
            var cullingResults = context.Cull(ref cullingParameters);

            // "LightMode"="ToyOpaque"のPassを使用する。
            var shaderTag = new ShaderTagId("ToyOpaque");

            // Cameraからソート設定を作る。自前で用意することもできるようになってそう。
            var sortingSetting = new SortingSettings(camera);
            // この辺よくわかってない。
            var drawingSettings = new DrawingSettings(shaderTag, sortingSetting);
            // この辺よくわかってない2。
            var filteringSettings = FilteringSettings.defaultValue;

            // カメラの行列を GlobalShaderProperty にセットする。
            context.SetupCameraProperties(camera);

            cmd.SetRenderTarget(camera.targetTexture);
            // クリアする (Clearフラグをちゃんと見た方がよさそう）
            if (camera.clearFlags != CameraClearFlags.Nothing)
            {
                cmd.ClearRenderTarget(true, true, default);
            }
            context.ExecuteCommandBuffer(cmd);

            // draw
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            // opaqueの後にSkyboxをレンダリングする
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }

#if UNITY_EDITOR
            // scene viewならギズモをレンダリングする。
            if (camera.cameraType == CameraType.SceneView)
            {
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
#endif

            // 実行。
            context.Submit();

            // コマンドバッファの解放
            CommandBufferPool.Release(cmd);
        }


        void RenderCubemap(ScriptableRenderContext context, Camera camera)
        {
            for (var i = 0; i < 6; i++)
            {
                var face = (CubemapFace) i;
                // NOTE: 上下反転してそう
                var rot = face switch
                {
                    CubemapFace.PositiveX => Quaternion.Euler(0, 90, 0),
                    CubemapFace.NegativeX => Quaternion.Euler(0, -90, 0),
                    CubemapFace.PositiveY => Quaternion.Euler(90, 0, 0),
                    CubemapFace.NegativeY => Quaternion.Euler(-90, 0, 0),
                    CubemapFace.PositiveZ => Quaternion.Euler(0, 0, 0),
                    CubemapFace.NegativeZ => Quaternion.Euler(0, 180, 0),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var cmd = CommandBufferPool.Get(ToyCubemap);
                camera.transform.rotation = rot;

                // カメラのカリングパラメータを取得する。
                if (!camera.TryGetCullingParameters(out var cullingParameters)) return;
                // カリングを実行。
                var cullingResults = context.Cull(ref cullingParameters);

                // "LightMode"="ToyOpaque"のPassを使用する。
                var shaderTag = new ShaderTagId("ToyOpaque");

                // Cameraからソート設定を作る。自前で用意することもできるようになってそう。
                var sortingSetting = new SortingSettings(camera);
                // この辺よくわかってない。
                var drawingSettings = new DrawingSettings(shaderTag, sortingSetting);
                // この辺よくわかってない2。
                var filteringSettings = FilteringSettings.defaultValue;

                // カメラの行列を GlobalShaderProperty にセットする。
                context.SetupCameraProperties(camera);
                // RenderTargetの指定はDrawRenderersの直前にやらないと駄目そう？
                cmd.SetRenderTarget(asset.cubemap, 0, (CubemapFace) i);
                cmd.ClearRenderTarget(true, false, default);
                context.ExecuteCommandBuffer(cmd);

                // draw
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                // opaqueの後にSkyboxをレンダリングする
                if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                {
                    context.DrawSkybox(camera);
                }

                // 実行。
                context.Submit();

                // コマンドバッファの解放
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
