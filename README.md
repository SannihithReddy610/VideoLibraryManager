# VideoLibraryManager
This is a WPF application (following the MVVM pattern) designed to view and manage videos. It includes the following features:

1. *Video Management:*

- Lists all available cloud and local videos (".mp4", ".avi", ".mkv") from "C:\Users" and "D:\", grouped by folders.
- Refresh button to reload the video library.
- Search bar to filter local videos.
- Displays metadata (full path, duration, file size, date modified) on hover for each local video.

2. *Playback Controls:*

- Play (on double-click), pause, stop, and resume functionality.
- Seek bar for navigating through the video and displaying the video duration.
- Slider to adjust the volume level.
- Play video in full screen.

3. *Context Menu Options (Right-Click on Local Videos):*

- Rename video.
- Delete video.
- Upload video files to the cloud.

4. *Context Menu Options (Right-Click on Cloud Videos):*

- Upload new version.
- Download video.
- Download previous version of video.
- Delete video.

5. *Cloud Operations:*

- Upload video files to the cloud.
- Upload a new version of an existing video to the cloud (this updates the video on the cloud and saves the current version for future retrieval).
- Download video files from the cloud.
- Download previous version of a selected video file from the cloud.
- Delete video files on the cloud (by default, this deletes the associated previous version as well).

6. *User Notifications:*

- Status bar to display notification messages to the user.
- Pop-up messages to notify the user about upload, download, and delete statuses.

**Dependencies**

1. Dotnet 8.0
2. Jfrog Cloud Platform
3. Third party packages [CommunityToolkit.Mvvm, Microsoft.Extensions.Configuration, Microsoft.Extensions.Logging, NAudio]

**Steps to launch the application**

1. Clone this repository.
2. Create a JFrog Cloud Platform instance to back up the video files [https://jfrog.com/start/].
3. Launch the created instance, create an artifact, and obtain the artifact key and artifact URL.
4. Create an environment variable named JFROG_API_KEY and place the artifact URL in the appsettings.json file located in the repository's root directory.
5. Build the code and launch the application.

**Configuration**

1. Modify "ArtifactoryUrl" in appsettings.json file with the artifact URL created before.
2. Modify the RootPaths section in the appsettings.json file to set the directories from which videos need to be displayed in the application.
3. Modify the VideoExtensions section in the appsettings.json file to specify the types of videos to be searched for.

**Sample use cases**

1. *On launch:* The application displays a list of all video files available from the configured RootPaths.
    - Note: Video files from local and cloud sources are displayed separately.
2. *Play a video:* Double-click on any video file listed under the local video files section to play the video.
3. *Control playback:* Use the UI buttons to control playback options, including Play, Pause, Stop, Full Screen, and dragging the seek bar to forward.
4. *Manage local video files:* Right-click on videos in the local video files section to rename, delete, and upload the file to the cloud.
5. *Manage cloud video files:* Right-click on videos in the cloud video files section to download, delete, upload a new version, and download a previous version.
    - On clicking the upload new version option, the current version of the video file is updated with the new video selected by the user.
    - The old version of the video can be retrieved using the download old version option.
    - Note: Deleting a video on the cloud will also delete the associated old versions.
