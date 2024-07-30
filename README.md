# VideoLibraryManager
This is a WPF application (following the MVVM pattern) designed to view and manage videos. It includes the following features:

1. Video Management:

- Lists all available cloud and local videos (".mp4", ".avi", ".mkv") from "C:\Users" and "D:\", grouped by folders.
- Refresh button to reload the video library.
- Search bar to filter local videos.
- Displays metadata (full path, duration, file size, date modified) on hover for each local video.

2. Playback Controls:

- Play (on double-click), pause, stop, and resume functionality.
- Seek bar for navigating through the video and displaying the video duration.
- Slider to adjust the volume level.
- Play video in full screen.

3. Context Menu Options (Right-Click on Local Videos):

- Rename video.
- Delete video.
- Upload video files to the cloud.

4. Context Menu Options (Right-Click on Cloud Videos):

- Upload new version.
- Download video.
- Download previous version of video.
- Delete video.

5. Cloud Operations:

- Upload video files to the cloud.
- Upload a new version of an existing video to the cloud (this updates the video on the cloud and saves the current version for future retrieval).
- Download video files from the cloud.
- Download previous version of a selected video file from the cloud.
- Delete video files on the cloud (by default, this deletes the associated previous version as well).

6. User Notifications:

- Status bar to display notification messages to the user.
- Pop-up messages to notify the user about upload, download, and delete statuses.