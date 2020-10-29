#------------------------------------------------------------------------------#
#                             Project files settings                           #
#------------------------------------------------------------------------------#

HEADERS             +=                                                      \
                        $$PWD/MainWindow.hpp                                \
                        $$PWD/MediaSubWindow.hpp                            \
    $$PWD/MediaSubWindowStyle.hpp

SOURCES             +=                                                      \
                        $$PWD/MainWindow.cpp                                \
                        $$PWD/MediaSubWindow.cpp                            \
    $$PWD/MediaSubWindowStyle.cpp \
                        $$PWD/main.cpp                                      \

FORMS               +=                                                      \
                        $$PWD/MainWindow.ui                                 \
                        $$PWD/MediaSubWindow.ui                             \

RESOURCES           +=                                                      \
                        $$PWD/resources.qrc                                 \
