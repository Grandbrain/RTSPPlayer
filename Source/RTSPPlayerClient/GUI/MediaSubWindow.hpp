#ifndef MEDIASUBWINDOW_HPP
#define MEDIASUBWINDOW_HPP

#include <QMdiSubWindow>

namespace Ui {
    class MediaSubWindow;
}

class MediaSubWindow : public QMdiSubWindow
{
    Q_OBJECT

public:
    explicit MediaSubWindow(QWidget *parent = nullptr);
    ~MediaSubWindow();

public slots:

    QColor titleBarColor() const;
    void setTitleBarColor(const QColor& color);

private:
    QColor titleColor_;
    QStyle* customStyle_;
    Ui::MediaSubWindow *ui;
};

#endif
