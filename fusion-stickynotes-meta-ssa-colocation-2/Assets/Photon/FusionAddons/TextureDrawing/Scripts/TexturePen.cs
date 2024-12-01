using Fusion.Addons.BlockingContact;
using Fusion.XR.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.TextureDrawing
{
    /***
     * 
     * The `TexturePen` is located on the pen (with a `BlockableTip` component) and try to detect a contact with a `TextureDrawing` (with a `BlockingSurface` component).
     * When a contact is detected, the local list of points to draw is updated. Then for each point, the method `AddDrawingPoint()` of `TextureDrawer` is called during the `FixedUpdateNetwork()`.
     * 
     ***/


    // Define a structure for music sheet notes
    public struct MusicNote
    {
        public Vector2 Center; // Center of the drawing
        public float Time;     // Timestamp of when the note was drawn
        public string Note;    // Calculated note (e.g., "C", "D")
    }

    public class TexturePen : NetworkBehaviour
    {

        private List<Vector2> currentDrawingPoints = new List<Vector2>();


        // private List<MusicNote> musicSheet = new List<MusicNote>();


        BlockableTip blockableTip;
        TextureDrawer textureDrawer;



        BlockingSurface lastBlockingsurface;
        TextureDrawing lastTextureDrawing;
        bool isDrawing = false;

        public Color color = Color.black;

        IColorProvider colorProvider;
        IFeedbackHandler feedback;

        [Header("Feedback")]
        [SerializeField] string audioType;
        [SerializeField] float hapticAmplitudeFactor = 0.1f;
        [SerializeField] FeedbackMode feedbackMode = FeedbackMode.AudioAndHaptic;

        public struct PendingDrawingPoint
        {
            public Vector2 position;
            public Color color;
            public byte pressureByte;
            public TextureDrawing drawing;
            public bool alreadyDrawn;
        }

        private void Awake()
        {
            feedback = GetComponent<IFeedbackHandler>();
            blockableTip = GetComponent<BlockableTip>();
            textureDrawer = GetComponent<TextureDrawer>();
            colorProvider = GetComponent<IColorProvider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Object && colorProvider != null)
                color = colorProvider.CurrentColor;

            if (Object == null || Object.HasStateAuthority == false) return;

            TextureDrawing previousTextureDrawing = lastTextureDrawing;
            bool wasDrawing = isDrawing;

            lastTextureDrawing = null;
            isDrawing = false;

            if (blockableTip.IsContactAllowed && blockableTip.IsInContact && blockableTip.lastSurfaceInContact != null)
            {
                TextureDrawing currentDrawing = null;

                if (blockableTip.lastSurfaceInContact != lastBlockingsurface || previousTextureDrawing == null)
                {
                    lastBlockingsurface = blockableTip.lastSurfaceInContact;
                    currentDrawing = blockableTip.lastSurfaceInContact.GetComponentInParent<TextureDrawing>();
                }
                else
                {
                    currentDrawing = previousTextureDrawing;
                }


                if (currentDrawing)
                {
                    lastTextureDrawing = currentDrawing;
                    isDrawing = true;

                    float blockableTipPressure = 0;
                    if (blockableTip.lastSurfaceInContact.maxDepth == 0)
                    {
                        blockableTipPressure = 1;
                    }
                    else
                    {
                        var depth = blockableTip.lastSurfaceInContact.referential.InverseTransformPoint(blockableTip.tip.position).z;
                        blockableTipPressure = Mathf.Clamp01(1f - ((blockableTip.lastSurfaceInContact.maxDepth - depth) / blockableTip.lastSurfaceInContact.maxDepth));
                    }

                    byte pressure = (byte)(1 + (byte)(254 * blockableTipPressure));
                    var coordinate = blockableTip.SurfaceContactCoordinates;
                    var surface = lastTextureDrawing.textureSurface;
                    Vector2 textureCoord = new Vector2(surface.TextureWidth * (coordinate.x + 0.5f), surface.TextureHeight * (0.5f - coordinate.y));
                    var newCoords = new Vector2(coordinate.x + 0.5f, 0.5f - coordinate.y);

                    // Track the drawing point
                    currentDrawingPoints.Add(newCoords);


                    textureDrawer.AddPointWithThrottle(textureCoord, pressure, color, lastTextureDrawing);



                    if (feedback != null)
                    {
                        feedback.PlayAudioAndHapticFeeback(audioType: audioType, audioOverwrite: false, hapticAmplitude: Mathf.Clamp01(hapticAmplitudeFactor * blockableTipPressure), feedbackMode: feedbackMode);
                    }
                }
            }

            if (wasDrawing && previousTextureDrawing != null && lastTextureDrawing != previousTextureDrawing)
            {
                // Add stop point
                textureDrawer.AddStopDrawingPointWithThrottle(previousTextureDrawing);
            }

            if (wasDrawing && isDrawing == false)
            {
                if (currentDrawingPoints.Count > 0)
                {
                    // Compute the center of the drawing
                    Vector2 center = Vector2.zero;
                    foreach (var point in currentDrawingPoints)
                    {
                        center += point;
                    }
                    center /= currentDrawingPoints.Count;

                    // Map the center to a note
                    string note = MapPositionToNote(center);

                    // Save the note data
                    // musicSheet.Add(new MusicNote
                    // {
                    //     Center = center,
                    //     Time = Time.time,
                    //     Note = note
                    // });
                    MusicNote noteObj = new MusicNote { Center = center, Time = Time.time, Note = note };
                    textureDrawer.AddMusicNote(noteObj, previousTextureDrawing);
                    // Clear the current drawing points for the next drawing
                    currentDrawingPoints.Clear();
                }

                if (feedback != null)
                {
                    feedback.StopAudioFeeback();
                }
            }


        }

        private string MapPositionToNote(Vector3 center)
        {
            // Notes from bottom (D) to top (G)
            string[] notes = { "D4", "E4", "F4", "G4", "A4", "B4", "C5", "D5", "E5", "F5", "G5" };

            // Normalize the Y position between 0 and 1
            float normalizedY = Mathf.Clamp01(-center.y + 1); // Clamp to ensure valid input

            // Calculate which note this position maps to
            int noteIndex = Mathf.FloorToInt(normalizedY * notes.Length);

            // Clamp the index to ensure it's within bounds
            noteIndex = Mathf.Clamp(noteIndex, 0, notes.Length - 1);

            // Return the corresponding note
            return notes[noteIndex];
        }
    }

}

