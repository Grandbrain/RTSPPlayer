/// \file Window.cpp
/// \brief Contains classes and functions definitions that provide main window
/// \brief implementation.
/// \bug No known bugs.

#include "Window.hpp"
#include "ui_Window.h"

///
/// \details
/// \param[in]	parent
Window::Window(QWidget* parent)
	: QMainWindow(parent),
	  ui_(new Ui::Window),
	  serializer_(Utilities::Serializers::NetworkSerializer::ByteOrder::LittleEndian) {

	ui_->setupUi(this);
}

///
/// \details
Window::~Window() {
	delete ui_;
	process_.write("command=close\n");
	process_.waitForFinished();
}

///
/// \details
/// \retval true on success.
/// \retval false on error.
void Window::initialize() {
	socket_.bind(50000);

	decoder_.initialize(Decoders::VideoDecoder::Codec::H264,
						Decoders::VideoDecoder::Format::RGB888);

	connect(&socket_, &QUdpSocket::readyRead, this, &Window::onDatagram);
	connect(&decoder_, &Decoders::VideoDecoder::onFrame, this, &Window::onImageFrame);
	connect(&process_, &QProcess::started, this, &Window::onProcessStart);
	connect(&process_, &QProcess::readyRead, this, &Window::onProcessRead);
	connect(&process_, &QProcess::errorOccurred, this, &Window::onProcessError);
	connect(ui_->pushButton, &QPushButton::released, this, &Window::onPushButton);

	QStringList args;
	args << "vidsrv" << "a2cam" << "v2cam";

	process_.setProcessChannelMode(QProcess::MergedChannels);
	process_.start("/home/andlom/Documents/Projects/Github/RTSPPlayer/Source/RTSPPlayerServer/RTSPPlayerServer/bin/Debug/netcoreapp3.1/RTSPPlayerServer", args);
	process_.waitForStarted();
}

///
/// \details
void Window::onDatagram() {
	while (socket_.hasPendingDatagrams()) {
		auto datagram = socket_.receiveDatagram();
		if (datagram.isNull()) continue;

		auto data = datagram.data();
		if (data.isEmpty()) continue;

		serializer_.deserialize(data);
	}

	QLinkedList<Utilities::Serializers::NetworkFrame> frames;
	serializer_.completedFrames(frames);

	for (auto&& frame : frames)
		onNetworkFrame(frame);
}

///
/// \details
/// \param[in]	frame
void Window::onNetworkFrame(const Utilities::Serializers::NetworkFrame& frame) {
	if (frame.flow != "v2cam1" || frame.data.size() <= 0) return;

	Utilities::Serializers::MemorySerializer serializer(frame.data);
	serializer.setByteOrder(Utilities::Serializers::MemorySerializer::ByteOrder::LittleEndian);

	quint8 isMetadataIncluded;
	serializer >> isMetadataIncluded;

	QByteArray configData, frameData;

	if (isMetadataIncluded > 0) {
		QByteArray frameTask(10, Qt::Uninitialized);
		serializer.readRawData(frameTask.data(), frameTask.size());

		serializer.skipRawData(4);

		qint32 configDataSize;
		serializer >> configDataSize;

		configData.resize(configDataSize);
		serializer.readRawData(configData.data(), configData.size());
	}

	qint32 frameDataSize = serializer.bytesAvailable();
	frameData.resize(frameDataSize);
	serializer.readRawData(frameData.data(), frameData.size());

	if (configData.size() > 0) {
		decoder_.setExtradata(configData);
	}

	if (frameData.size() > 0)
		decoder_.decode(frameData);
}

///
/// \details
/// \param[in]	images
void Window::onImageFrame(const QImage& frame) {
	ui_->label->setPixmap(QPixmap::fromImage(frame));
}

///
/// \details
void Window::onProcessStart() {
	if (process_.state() == QProcess::Running) {
		process_.write("command=add "
					   "name=1 "
					   "url=rtsp://192.168.11.23:8554/streamA.h264 "
					   "media=video\n");

		process_.write("command=set "
					   "name=1 "
					   "address=127.0.0.1 "
					   "port=50000\n");

		/*process_.write("command=start "
					   "name=1\n");*/
	}
}

///
/// \details
void Window::onProcessRead() {
	qDebug() << process_.read(process_.bytesAvailable());
}

///
/// \details
/// \param[in]	error
void Window::onProcessError(QProcess::ProcessError error) {

}

///
/// \details
void Window::onPushButton() {
	process_.write("command=start "
						   "name=1\n");
}
