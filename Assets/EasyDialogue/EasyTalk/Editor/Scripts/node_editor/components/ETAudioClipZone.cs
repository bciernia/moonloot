using EasyTalk.Editor.Utils;
using System;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETAudioClipZone : Box
    {
        private string chosenFile = null;

        private AudioClip audioClip = null;

        private GameObject audioObject;

        private bool isPlaying = false;

        private Thread audioThread;

        public ETAudioClipZone()
        {
            this.AddToClassList("audio-clip-zone");
            this.pickingMode = PickingMode.Position;
            this.RegisterCallback<DragExitedEvent>(OnDragExited);
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.RegisterCallback<AttachToPanelEvent>(OnAttachedToParent);
            this.RegisterCallback<DragEnterEvent>(OnDragEnter);
            this.RegisterCallback<DragLeaveEvent>(OnDragLeave);

            this.tooltip = "No audio selected. Drag and drop a sound file to use.";
        }

        public AudioClip GetAudioClip()
        {
            if (chosenFile != null && chosenFile.Length > 0)
            {
                return audioClip;
            }

            return null;
        }

        public string GetAudioClipFile()
        {
            return chosenFile;
        }

        public void SetAudioClip(int assetID)
        {
            string filePath = AssetDatabase.GetAssetPath(assetID);

            if (filePath != null && filePath.Length > 0)
            {
                SetAudioClipFile(filePath);
            }
        }

        public void SetAudioClipFile(string filePath)
        {
            this.chosenFile = filePath;
            this.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(chosenFile);

            if (this.audioClip == null && chosenFile != null && chosenFile.Contains("/"))
            {
                this.chosenFile = chosenFile.Substring(chosenFile.LastIndexOf("/") + 1);
                string[] filePaths = AssetDatabase.FindAssets(chosenFile);

                if (filePath.Length > 0)
                {
                    foreach (string path in filePaths)
                    {
                        this.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(chosenFile);
                        if (this.audioClip != null) { break; }
                    }
                }
            }

            if (audioClip != null)
            {
                this.tooltip = chosenFile;
                this.AddToClassList("audio-clip-zone-playable");
            }
        }

        public void SetAudioClip(AudioClip audioClip)
        {
            if (audioClip != null)
            {
                this.chosenFile = AssetDatabase.GetAssetPath(audioClip);
                this.audioClip = audioClip;
                this.tooltip = chosenFile;
                this.AddToClassList("audio-clip-zone-playable");
            }
        }

        private void OnAttachedToParent(AttachToPanelEvent evt)
        {
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            if (DragAndDrop.paths.Length == 0) { return; }

            string file = DragAndDrop.paths[0];

            if (file.EndsWith(".wav") || file.EndsWith(".ogg") || file.EndsWith(".mp3"))
            {
                EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.Generic);
            }
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.None);
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            if (DragAndDrop.paths.Length == 0) { return; }

            string file = DragAndDrop.paths[0];

            if (file.EndsWith(".wav") || file.EndsWith(".ogg") || file.EndsWith(".mp3"))
            {
                chosenFile = file;
                audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(chosenFile);
                this.tooltip = chosenFile;
                EasyTalkNodeEditor.Instance.NodesChanged();

                this.AddToClassList("audio-clip-zone-playable");
            }

            EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.None);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0 && audioClip != null)
            {
                if (!isPlaying)
                {
                    PlayAudioClip(audioClip);
                }
                else
                {
                    StopAudioSource();
                }
            }
            else if (evt.button == 1)
            {
                this.chosenFile = null;
                this.audioClip = null;
                this.tooltip = "No audio selected. Drag and drop a sound file to use.";
                this.RemoveFromClassList("audio-clip-zone-playable");
                StopAudioSource();
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }

        private void PlayAudioClip(AudioClip clip)
        {
            StopAudioSource();

            this.AddToClassList("audio-clip-zone-playing");
            isPlaying = true;
            audioObject = new GameObject();
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.gameObject.name = "NM AUDIO SOURCE";

            foreach (Component component in audioSource.GetComponents<Component>())
            {
                if (!(component is AudioSource) && !(component is Transform))
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }

            }

            audioSource.clip = clip;
            //audioSource.gameObject.hideFlags |= HideFlags.DontSave;
            audioSource.playOnAwake = false;
            audioSource.gameObject.SetActive(true);
            audioSource.enabled = true;
            audioSource.volume = EasyTalkNodeEditor.Instance.GetAudioVolume();
            audioSource.Play();

            float clipLength = clip.length;

            audioThread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep((int)(clipLength * 1000));

                    if (audioThread.ThreadState != ThreadState.Stopped)
                    {
                        EditorApplication.delayCall += StopAudioSource;
                    }
                }
                catch { }
            });

            audioThread.Start();
        }

        private void StopAudioSource()
        {
            if (audioObject != null)
            {
                if (audioThread != null)
                {
                    try
                    {
                        audioThread.Interrupt();
                    }
                    catch { }
                    audioThread = null;
                }

                audioObject.GetComponent<AudioSource>().Stop();
                UnityEngine.Object.DestroyImmediate(audioObject);

                this.RemoveFromClassList("audio-clip-zone-playing");
            }

            isPlaying = false;
        }

        public void PlayClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            MethodInfo method = audioUtilClass.GetMethod("PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public, null,
                new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);

            method.Invoke(null, new object[] { clip, 0, false });
        }

        public void StopClip()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            MethodInfo method = audioUtilClass.GetMethod("StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public, null,
                new System.Type[] { }, null);

            method.Invoke(null, new object[] { });
        }

        public void OnMouseMove(MouseMoveEvent evt)
        {
            Vector2 mousePos = (evt.target as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.mousePosition);

            VisualElement elementAtMouse = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);
            if (elementAtMouse == this || evt.target == this)
            {
                this.style.opacity = 1.0f;
            }
            else
            {
                this.style.opacity = 0.7f;
            }
        }
    }
}