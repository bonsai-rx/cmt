#pragma once

#include <CMT.h>
#include "opencv2/core/core.hpp"
using namespace System;

namespace Cmt
{
	namespace Net
	{
		public ref class Cmt
		{
		private:
			cmt::CMT *cmt;
		public:
			Cmt(void);
			void Initialize(OpenCV::Net::Arr ^image, OpenCV::Net::Rect rect);
			void ProcessFrame(OpenCV::Net::Arr ^image);
		};
	}
}

