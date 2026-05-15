// https://github.com/unity3d-jp/UnityChanBallRoll/blob/26ea0a89f71a1c4d71d708662ed2f139e3b66f3f/Assets/Etc/Effects/ImageEffects/Shaders/MotionBlurClear.shader
Shader "Hidden/MotionBlurClear" 
{
    Properties 
    {
        _ClearDistance ("Clear Distance (meters)", Float) = 10.0
    }

    SubShader 
    {
        Pass 
        {
            ZTest Always
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct vs_input 
            {
                float4 vertex : POSITION;
            };

            struct ps_input 
            {
                float4 pos : SV_POSITION;
                float4 screen : TEXCOORD0;
            };

            sampler2D_float _CameraDepthTexture;
            float _ClearDistance;

            ps_input vert (vs_input v)
            {
                ps_input o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);    
                o.screen = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.screen.z);
                return o;
            }

            float4 frag (ps_input i) : SV_Target
            {
                float sceneDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screen));
                sceneDepth = LinearEyeDepth(sceneDepth);

                float pixelDepth = i.screen.z;

                clip(sceneDepth - pixelDepth + 1e-2);

				clip(_ClearDistance - sceneDepth);


                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    Fallback Off
}