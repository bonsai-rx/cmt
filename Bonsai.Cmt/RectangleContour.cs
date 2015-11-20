using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Cmt
{
    class RectangleContour : Contour
    {
        const uint CV_MAGIC_MASK = 0xFFFF0000;
        const uint CV_SEQ_MAGIC_VAL = 0x42990000;
        static readonly int PointSize = Marshal.SizeOf(typeof(Point));
        static readonly int SeqBlockSize = Marshal.SizeOf(typeof(_CvSeqBlock));
        static readonly new int HeaderSize = Contour.HeaderSize + SeqBlockSize + PointSize * 4;

        public RectangleContour(RotatedRect rect, Size imageSize)
        {
            var size = rect.Size;
            var center = rect.Center;
            var angle = rect.Angle * Math.PI / 180f;
            var a = 0.5f * (float)Math.Sin(angle);
            var b = 0.5f * (float)Math.Cos(angle);

            unsafe
            {
                var ptr = Marshal.AllocHGlobal(HeaderSize);
                var points = (int*)((byte*)ptr + Contour.HeaderSize + SeqBlockSize);
                points[0] = Math.Max(0, Math.Min(imageSize.Width - 1, (int)(center.X - a * size.Height - b * size.Width)));
                points[1] = Math.Max(0, Math.Min(imageSize.Height - 1, (int)(center.Y + b * size.Height - a * size.Width)));
                points[2] = Math.Max(0, Math.Min(imageSize.Width - 1, (int)(center.X + a * size.Height - b * size.Width)));
                points[3] = Math.Max(0, Math.Min(imageSize.Height - 1, (int)(center.Y - b * size.Height - a * size.Width)));
                points[4] = Math.Max(0, Math.Min(imageSize.Width - 1, (int)(2 * center.X - points[0])));
                points[5] = Math.Max(0, Math.Min(imageSize.Height - 1, (int)(2 * center.Y - points[1])));
                points[6] = Math.Max(0, Math.Min(imageSize.Width - 1, (int)(2 * center.X - points[2])));
                points[7] = Math.Max(0, Math.Min(imageSize.Height - 1, (int)(2 * center.Y - points[3])));

                Rect boundingRect;
                var minX = Math.Min(Math.Min(Math.Min(points[0], points[2]), points[4]), points[6]);
                var minY = Math.Min(Math.Min(Math.Min(points[1], points[3]), points[5]), points[7]);
                var maxX = Math.Max(Math.Max(Math.Max(points[0], points[2]), points[4]), points[6]);
                var maxY = Math.Max(Math.Max(Math.Max(points[1], points[3]), points[5]), points[7]);
                boundingRect.X = minX;
                boundingRect.Y = minY;
                boundingRect.Width = maxX - minX + 1;
                boundingRect.Height = maxY - minY + 1;

                var seqBlock = (_CvSeqBlock*)((byte*)ptr + Contour.HeaderSize);
                seqBlock->start_index = 0;
                seqBlock->count = 4;
                seqBlock->data = (IntPtr)points;
                seqBlock->next = (IntPtr)seqBlock;
                seqBlock->prev = (IntPtr)seqBlock;

                var contour = (_CvContour*)ptr;
                var seqFlags = (int)SequenceElementType.Point | (int)SequenceKind.Curve | (int)SequenceFlags.Closed;
                contour->flags = (int)((seqFlags & ~CV_MAGIC_MASK) | CV_SEQ_MAGIC_VAL);
                contour->header_size = Contour.HeaderSize;
                contour->h_next = IntPtr.Zero;
                contour->h_prev = IntPtr.Zero;
                contour->v_next = IntPtr.Zero;
                contour->v_prev = IntPtr.Zero;
                contour->total = 4;
                contour->elem_size = Marshal.SizeOf(typeof(Point));
                contour->block_max = (IntPtr)((byte*)ptr + HeaderSize);
                contour->ptr = contour->block_max;
                contour->delta_elems = 128;
                contour->storage = IntPtr.Zero;
                contour->free_blocks = IntPtr.Zero;
                contour->first = (IntPtr)seqBlock;
                contour->color = 0;
                contour->rect = boundingRect;
                contour->reserved0 = 0;
                contour->reserved1 = 0;
                contour->reserved2 = 0;
                SetHandle(ptr);
            }
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct _CvSeqBlock
        {
            public IntPtr prev;
            public IntPtr next;
            public int start_index;
            public int count;
            public IntPtr data;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct _CvContour
        {
            public int flags;
            public int header_size;
            public IntPtr h_prev;
            public IntPtr h_next;
            public IntPtr v_prev;
            public IntPtr v_next;
            public int total;
            public int elem_size;
            public IntPtr block_max;
            public IntPtr ptr;
            public int delta_elems;
            public IntPtr storage;
            public IntPtr free_blocks;
            public IntPtr first;

            public Rect rect;
            public int color;
            public int reserved0;
            public int reserved1;
            public int reserved2;
        }
    }
}
