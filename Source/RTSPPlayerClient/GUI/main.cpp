/// \file main.cpp
/// \brief Contains entry point to the application.
/// \bug No known bugs.

#include "MainWindow.hpp"

#include <QApplication>

/// Runs the main application thread.
/// \details Starts the main application window.
/// \param[in]  argc    Number of arguments passed to the program.
/// \param[in]  argv    An array of pointers to the arguments passed to the
///                     program.
/// \return Exit status.
int main(int argc, char *argv[]) {
    QApplication app(argc, argv);

    MainWindow mainWindow;
    mainWindow.show();

    return app.exec();
}
