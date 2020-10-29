#include "MainWindow.hpp"
#include "MediaSubWindow.hpp"

#include "ui_MainWindow.h"

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);

    auto mediaSubWindow = new MediaSubWindow;
    mediaSubWindow->setTitleBarColor(QColor("lightgray"));
    ui->mdiArea->addSubWindow(mediaSubWindow);
}

MainWindow::~MainWindow()
{
    delete ui;
}
