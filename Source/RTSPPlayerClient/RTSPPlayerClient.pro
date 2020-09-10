#------------------------------------------------------------------------------#
#                                Base settings                                 #
#------------------------------------------------------------------------------#

TEMPLATE            =   app
TARGET              =   rtspplayerclient
QT                  +=  widgets network
CONFIG              +=	c++17 strict_c++


#------------------------------------------------------------------------------#
#                                Project macros                                #
#------------------------------------------------------------------------------#

DEFINES             +=                                                      \
                        QT_DEPRECATED_WARNINGS                              \
                        NETWORK_PROTOCOL_EXTENDED=0                         \


#------------------------------------------------------------------------------#
#                            Project subdirectories                            #
#------------------------------------------------------------------------------#

include($$absolute_path(Common.pri, Common))
include($$absolute_path(GUI.pri, GUI))
include($$absolute_path(Playback.pri, Playback))


#------------------------------------------------------------------------------#
#                            External dependencies                             #
#------------------------------------------------------------------------------#

FFMPEG_DIRECTORY    =   $$find_directory($$EXTERNAL_PATH, "ffmpeg-*")
FFMPEG_INCLUDE_PATH =   $$find_include_path($$EXTERNAL_PATH, $$FFMPEG_DIRECTORY)
FFMPEG_LIBRARY_PATH =   $$find_library_path($$EXTERNAL_PATH, $$FFMPEG_DIRECTORY)


#------------------------------------------------------------------------------#
#                          Include directories settings                        #
#------------------------------------------------------------------------------#

INCLUDEPATH         +=                                                      \
                        $$FFMPEG_INCLUDE_PATH                               \


DEPENDPATH          +=                                                      \
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
