#import <UIKit/UIKit.h>

extern "C"
{
    void _SaveVideoToPhotosAlbum(const char* videoPath)
    {
        if (videoPath == NULL)
            return;

        NSString* path = [NSString stringWithUTF8String:videoPath];
        if (path == nil || path.length == 0)
            return;

        UISaveVideoAtPathToSavedPhotosAlbum(path, nil, nil, nil);
    }
}
