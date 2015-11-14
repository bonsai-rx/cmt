#pragma once

#include <CMT.h>
#include "opencv2/core/core.hpp"
using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace Cmt
{
	namespace Net
	{
		public ref class Cmt
		{
		private:
			cmt::CMT *cmt;
		public:
			Cmt();
			~Cmt();
			!Cmt();
			void Initialize(OpenCV::Net::Arr ^image, OpenCV::Net::Rect rect);
			void ProcessFrame(OpenCV::Net::Arr ^image);

			property OpenCV::Net::RotatedRect BoundingBox
			{
				OpenCV::Net::RotatedRect get()
				{
					return OpenCV::Net::RotatedRect(
						OpenCV::Net::Point2f(cmt->bb_rot.center.x, cmt->bb_rot.center.y),
						OpenCV::Net::Size2f(cmt->bb_rot.size.width, cmt->bb_rot.size.height),
						cmt->bb_rot.angle);
				}
			}

			property System::Collections::Generic::IEnumerable<OpenCV::Net::Point2f> ^ ActivePoints
			{
				System::Collections::Generic::IEnumerable<OpenCV::Net::Point2f> ^ get()
				{
					return gcnew ActivePointEnumerable(cmt);
				}
			}

		private:
			ref class ActivePointEnumerable : public System::Collections::Generic::IEnumerable<OpenCV::Net::Point2f>
			{
			private:
				cmt::CMT *cmt;
			internal:
				ActivePointEnumerable(cmt::CMT *cmt):
					cmt(cmt)
				{
				}

			public:
				virtual System::Collections::IEnumerator ^ EnumerableGetEnumerator() =
					System::Collections::IEnumerable::GetEnumerator
				{
					return this->GenericEnumerableGetEnumerator();
				}

				virtual System::Collections::Generic::IEnumerator<OpenCV::Net::Point2f> ^ GenericEnumerableGetEnumerator() =
					System::Collections::Generic::IEnumerable<OpenCV::Net::Point2f>::GetEnumerator
				{
					return gcnew ActivePointEnumerator(cmt);
				}
			};

			ref class ActivePointEnumerator : public System::Collections::Generic::IEnumerator<OpenCV::Net::Point2f>
			{
			private:
				int index;
				cmt::CMT *cmt;

			internal:
				ActivePointEnumerator(cmt::CMT *cmt):
					cmt(cmt),
					index(-1)
				{
				}

				property Object ^ CurrentNonGeneric {
					virtual Object ^ get() = System::Collections::IEnumerator::Current::get { return Current; }
				}

			public:
				~ActivePointEnumerator() { }
				virtual void Reset() { index = -1; }

				property OpenCV::Net::Point2f Current {
					virtual OpenCV::Net::Point2f get() { return *((OpenCV::Net::Point2f*)&cmt->points_active.at(index)); }
				}

				virtual bool MoveNext()
				{
					return ++index < cmt->points_active.size();
				}
			};
		};
	}
}

