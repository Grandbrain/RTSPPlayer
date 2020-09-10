#include "PlaybackVideo.hpp"
#include <QDebug>

PlaybackVideo::PlaybackVideo(QWidget *parent) : QWidget(parent)
{

}

void PlaybackVideo::setImage(const QImage& image) {
	image_ = image;
}

void PlaybackVideo::paintEvent(QPaintEvent *)
{
	qDebug() << "hehe";
	QPainter painter(this);
	painter.drawImage(rect(), image_);
}
