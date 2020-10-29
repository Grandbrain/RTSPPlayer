#------------------------------------------------------------------------------#
#                                Base settings                                 #
#------------------------------------------------------------------------------#

TEMPLATE            =   app
TARGET              =   rtspplayerclient
QT                  +=  widgets
CONFIG              +=	c++17 strict_c++


#------------------------------------------------------------------------------#
#                                Project macros                                #
#------------------------------------------------------------------------------#

DEFINES             +=                                                      \
                        ASIO_STANDALONE                                     \
                        NETWORK_PROTOCOL_EXTENDED=0                         \


#------------------------------------------------------------------------------#
#                            Project subdirectories                            #
#------------------------------------------------------------------------------#

include($$absolute_path(Base.pri, Base))
include($$absolute_path(GUI.pri, GUI))
include($$absolute_path(Playback.pri, Playback))


#------------------------------------------------------------------------------#
#                            External dependencies                             #
#------------------------------------------------------------------------------#

ASIO_DIRECTORY      =   $$find_directory($$EXTERNAL_PATH, "asio-*")
ASIO_INCLUDE_PATH   =   $$find_include_path($$EXTERNAL_PATH, $$ASIO_DIRECTORY)

FFMPEG_DIRECTORY    =   $$find_directory($$EXTERNAL_PATH, "ffmpeg-*")
FFMPEG_INCLUDE_PATH =   $$find_include_path($$EXTERNAL_PATH, $$FFMPEG_DIRECTORY)
FFMPEG_LIBRARY_PATH =   $$find_library_path($$EXTERNAL_PATH, $$FFMPEG_DIRECTORY)


#------------------------------------------------------------------------------#
#                          Include directories settings                        #
#------------------------------------------------------------------------------#

INCLUDEPATH         +=                                                      \
                        $$ASIO_INCLUDE_PATH                                 \
                        $$FFMPEG_INCLUDE_PATH                               \


DEPENDPATH          +=                                                      \
                        $$ASIO_INCLUDE_PATH                                 \
                        $$FFMPEG_INCLUDE_PATH                               \


#------------------------------------------------------------------------------#
#                           External libraries settings                        #
#------------------------------------------------------------------------------#

LIBS                +=                                                      \
                        -L$$FFMPEG_LIBRARY_PATH                             \

LIBS                +=                                                      \
                        -lavcodec                                           \
                        -lavdevice                                          \
                        -lavfilter                                          \
                        -lavformat                                          \
                        -lavutil                                            \
                        -lswresample                                        \
                        -lswscale                                           \
