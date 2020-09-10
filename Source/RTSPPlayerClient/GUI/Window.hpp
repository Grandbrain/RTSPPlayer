/// \file Window.hpp
/// \brief Contains classes and functions declarations that provide main window
/// implementation.
/// \bug No known bugs.

#ifndef WINDOW_H
#define WINDOW_H

#include "Decoders/VideoDecoder.hpp"
#include "Utilities/MemorySerializer.hpp"
#include "Utilities/NetworkSerializer.hpp"

#include <QProcess>
#include <QMainWindow>
#include <QUdpSocket>
#include <QNetworkDatagram>

///
namespace Ui {

	///
	class Window;
}

///
class Window : public QMainWindow {

	Q_OBJECT

public:

	///
	/// \param[in]	parent
	explicit Window(QWidget* parent = nullptr);

	///
	~Window() override;

public:

	///
	void initialize();

private slots:

	///
	void onDatagram();

	///
	/// \param[in]	frame
	void onNetworkFrame(const Utilities::Serializers::NetworkFrame& frame);

	///
	/// \param[in]	image
	void onImageFrame(const QImage& frame);

	///
	void onProcessStart();

	///
	void onProcessRead();

	///
	/// \param[in]	error
	void onProcessError(QProcess::ProcessError error);

	///
	void onPushButton();

private:

	///
	Ui::Window* ui_;

	///
	QProcess process_;

	///
	QUdpSocket socket_;

	///
	Decoders::VideoDecoder decoder_;

	///
	Utilities::Serializers::NetworkSerializer serializer_;
};

#endif
