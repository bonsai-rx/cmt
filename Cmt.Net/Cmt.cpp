#include "Cmt.h"


Cmt::Net::Cmt::Cmt(void):
cmt(new cmt::CMT())
{
}

void Cmt::Net::Cmt::Initialize(OpenCV::Net::Arr ^image, OpenCV::Net::Rect rect)
{
	if (image == nullptr)
	{
		throw gcnew ArgumentNullException("image");
	}

	IntPtr handle = image->DangerousGetHandle();
	cv::Mat cvimage = cv::cvarrToMat(handle.ToPointer());
	cmt->initialize(cvimage, cv::Rect(rect.X, rect.Y, rect.Width, rect.Height));
}

void Cmt::Net::Cmt::ProcessFrame(OpenCV::Net::Arr ^image)
{
	if (image == nullptr)
	{
		throw gcnew ArgumentNullException("image");
	}

	IntPtr handle = image->DangerousGetHandle();
	cv::Mat cvimage = cv::cvarrToMat(handle.ToPointer());
	cmt->processFrame(cvimage);
}