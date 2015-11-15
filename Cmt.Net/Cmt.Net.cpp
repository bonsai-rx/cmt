#include "Cmt.Net.h"


Cmt::Net::Cmt::Cmt():
cmt(new cmt::CMT())
{
}

Cmt::Net::Cmt::!Cmt()
{
	delete cmt;
	previous = nullptr;
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
	previous = image;
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
	previous = image;
}