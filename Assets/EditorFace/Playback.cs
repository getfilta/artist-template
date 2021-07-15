using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

[ExecuteAlways]
public class Playback : MonoBehaviour
{
    private long recordingLength;

    private FaceRecording faceRecording;
    private long prevTime;
    private int previousFrame;

    public SkinnedMeshRenderer skinnedMeshRenderer;
    private DateTime startTime;
    private bool isPlayingBack;
    private void Update(){
        if (skinnedMeshRenderer == null)
            return;
        if (faceRecording.faceDatas == null){
            try{
                GetRecordingData();
            }
            catch (Exception e){
                Debug.Log($"Could not get recorded face data. {e.Message}");
                throw;
            }
            
        }
        if (!isPlayingBack){
            startTime = DateTime.Now;
            isPlayingBack = true;
            Debug.Log("Starting playback");
        }
        if (isPlayingBack){
            long time = (long)(DateTime.Now - startTime).TotalMilliseconds;
            Play(time);
        }
    }

    void Replay(){
        startTime = DateTime.Now;
    }

    void GetRecordingData(){
        string path = Path.Combine(Application.dataPath, "EditorFace", "FaceRecording");
        byte[] data = File.ReadAllBytes(path);
        string faceData = Encoding.ASCII.GetString(data);
        faceRecording = JsonConvert.DeserializeObject<FaceRecording>(faceData);
        recordingLength = faceRecording.faceDatas[faceRecording.faceDatas.Count - 1].timestamp;
    }

    private void OnDrawGizmos(){
#if UNITY_EDITOR
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    private void Play(long currentTime)
        {
            if (recordingLength <= 0) { return; }
            if (currentTime > recordingLength)
            {
                Replay();
                return;
            }
            if (prevTime > currentTime)
            {
                previousFrame = 0;
            }
            prevTime = currentTime;

            for (int i = previousFrame; i < faceRecording.faceDatas.Count; i++){
                FaceData faceData = faceRecording.faceDatas[i];
                long nextTimeStamp = faceData.timestamp;
                float[] nextBlendShape = faceData.blendshapeData;

                //we want to find the timestamp in the future so we can walk back a frame and interpolate
                if (nextTimeStamp < currentTime)
                {
                    if (i == faceRecording.faceDatas.Count - 1)
                    {
                        i = 0;
                        break;
                    }
                    //we haven't found the future yet. try the next one.
                    continue;
                }
                if (i == 0)
                {
                    break;
                }

                FaceData prevFaceData = faceRecording.faceDatas[i - 1];
                long prevTimeStamp = prevFaceData.timestamp;
                float[] prevBlendShape = prevFaceData.blendshapeData;
                float nextWeight = (float)(currentTime - prevTimeStamp) / (nextTimeStamp - prevTimeStamp);
                float prevWeight = 1f - nextWeight;
                skinnedMeshRenderer.transform.localPosition = faceData.face.localPosition;
                skinnedMeshRenderer.transform.localEulerAngles = faceData.face.localRotation;
                //now to grab the blendshape values of the prev and next frame and lerp + assign them
                for (int j = 0; j < prevBlendShape.Length - 2; j++)
                {
                    var nowValue = (prevBlendShape[j] * prevWeight) + (nextBlendShape[j] * nextWeight);
                    skinnedMeshRenderer.SetBlendShapeWeight(j, nowValue);
                }

                previousFrame = i;
                break;
            }
        }
    
    public struct FaceData
    {
        public long timestamp;
        public float[] blendshapeData;
        public Trans face;
        public Trans leftEye;
        public Trans rightEye;

        public struct Trans
        {
            public Vector3Json position;
            public Vector3Json rotation;
            public Vector3Json localPosition;
            public Vector3Json localRotation;
                
            public static implicit operator Trans(Transform trans){
                return new Trans{position = trans.position, rotation = trans.eulerAngles, localPosition = trans.localPosition, localRotation = trans.localEulerAngles};
            }

            public struct Vector3Json
            {
                public float x, y, z;
                public static implicit operator Vector3Json(Vector3 vector){
                    return new Vector3Json{x = vector.x, y = vector.y, z = vector.z};
                }
                public static implicit operator Vector3(Vector3Json vector){
                    return new Vector3{x = vector.x, y = vector.y, z = vector.z};
                }
            }
        }
    }
    
    public struct FaceRecording
    {
        public List<FaceData> faceDatas;
    }
}
