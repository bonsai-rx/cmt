using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using Cmt.Net;
using Bonsai.Vision;

namespace Bonsai.Cmt
{
    [DefaultProperty("RegionOfInterest")]
    [Description("Tracks a specified object over time using the self-supervised CMT algorithm.")]
    public class CmtTracker : Transform<IplImage, TrackedComponent>
    {
        [Description("Indicates whether to estimate the rotation of the object.")]
        public bool EstimateRotation { get; set; }

        [Description("Indicates whether to estimate the scale of the object.")]
        public bool EstimateScale { get; set; }

        [Description("The region of interest to track inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        public override IObservable<TrackedComponent> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var tracker = new TrackerCmt();
                var inputRoi = new Rect();
                var initialized = false;
                return source.Select(input =>
                {
                    var frame = input;
                    if (input.Channels == 3)
                    {
                        frame = new IplImage(input.Size, input.Depth, 1);
                        CV.CvtColor(input, frame, ColorConversion.Bgr2Gray);
                    }

                    tracker.EstimateRotation = EstimateRotation;
                    tracker.EstimateScale = EstimateScale;
                    if (inputRoi != RegionOfInterest)
                    {
                        inputRoi = RegionOfInterest;
                        tracker.Initialize(frame, inputRoi);
                        initialized = true;
                    }

                    var component = new TrackedComponent();
                    if (initialized)
                    {
                        tracker.ProcessFrame(frame);
                        var boundingBox = tracker.BoundingBox;
                        component.Centroid = boundingBox.Center;
                        var boundingContour = new RectangleContour(boundingBox, frame.Size);
                        if (!float.IsNaN(boundingBox.Center.X) &&
                            boundingContour.Rect.Width > 0 &&
                            boundingContour.Rect.Height > 0)
                        {
                            component.Area = boundingBox.Size.Width * boundingBox.Size.Height;
                            component.Orientation = boundingBox.Angle * Math.PI / 180f;
                            component.Contour = boundingContour;
                            component.Patch = input.GetSubRect(component.Contour.Rect);
                            component.MajorAxisLength = 0;
                            component.MinorAxisLength = 0;
                        }
                        else component.Orientation = double.NaN;
                        component.Confidence = tracker.Confidence;
                    }
                    return component;
                });
            });
        }
    }
}
