#ifndef PLAYBACKVIDEO_HPP
#define PLAYBACKVIDEO_HPP

#include <QWidget>
#include <QPainter>

class PlaybackVideo : public QWidget
{
	Q_OBJECT
public:
	explicit PlaybackVideo(QWidget *parent = nullptr);
	void setImage(const QImage& image);

protected:
	void paintEvent(QPaintEvent *) override;

signals:

public slots:

private:
	QImage image_;
};

#endif
