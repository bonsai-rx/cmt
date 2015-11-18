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

namespace Bonsai.Cmt
{
    public class CmtTracker : Transform<IplImage, IplImage>
    {
        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var tracker = new TrackerCmt();
                var inputRoi = new Rect();
                var initialized = false;
                return source.Select(input =>
                {
                    var output = input.Clone();
                    //Initialize
                    if (inputRoi != RegionOfInterest)
                    {
                        inputRoi = RegionOfInterest;
                        tracker.Initialize(output, inputRoi);
                        initialized = true;
                    }

                    if (initialized)
                    {
                        tracker.ProcessFrame(output);
                        if (!float.IsNaN(tracker.BoundingBox.Center.X)) //When target is lost, bounding box is null
                        {
                            CV.EllipseBox(output, tracker.BoundingBox, Scalar.Rgb(0, 255, 0));
                        }
                    }
                    return output;
                });
            });
        }
    }
}
