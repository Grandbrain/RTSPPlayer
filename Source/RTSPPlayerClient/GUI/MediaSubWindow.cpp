#include "MediaSubWindow.hpp"
#include "MediaSubWindowStyle.hpp"

#include "ui_MediaSubWindow.h"

MediaSubWindow::MediaSubWindow(QWidget *parent) :
    QMdiSubWindow(parent),
    titleColor_(),
    customStyle_(new GUI::MediaSubWindowStyle(style())),
    ui(new Ui::MediaSubWindow)
{
    auto currentLayout = layout();
    delete currentLayout;

    ui->setupUi(this);
    setStyle(customStyle_);
}

MediaSubWindow::~MediaSubWindow()
{
    delete ui;
}


QColor MediaSubWindow::titleBarColor() const {
    return titleColor_;
}

void MediaSubWindow::setTitleBarColor(const QColor& color) {
    titleColor_ = color;
    update();
}
