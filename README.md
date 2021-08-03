# Filta Artist Template

Template to use for creating new Filta face filters. 

Includes a default lighting/rendering setup, a simulated face mesh for testing, and the "Filta Artist Panel" accessible via the Menu bar.

# OPENING THE TEMPLATE
1. Download the latest release
2. Open Unity Hub
3. Add the project (ADD button in the top right) and open the project
4. Open the "templateScene" file in the "Assets" folder
5. Create a filter!

# CREATING A FILTER
1. Adding objects as children of the FaceTracker (found as a child of the Filta object in the scene), ensures the objects follow the head of the user.
2. Adding objects as children of the LeftEyeTracker, RightEyeTracker or NoseBridgeTracker, ensures the objects follow those body parts respectively.
3. Adding SkinnedMeshes as children of the FaceMasks object, ensure the meshes follow the blendshape data gotten from the face of the user. This means that masks can be made to follow expressions (smile, blink, raise eyebrows) of the user.
4. You can add the DefaultFace prefab in DefaultAssets folder as children of the Faces object to ensure that face meshes are generated that to fit the face of the user. 
5. All your additions are properly simulated by the Simulator to give you an accurate preview of how your filter would look. The Simulator can be paused/played either in the "Filta Artist Panel" or in the Inspector (with the Simulator object clicked)

# UPLOADING A FILTER
1. Ensure that everything you have created is under the "Filter" object in the scene hierarchy
2. Open the "Artist Panel" via the menu bar (Filta->Artist Panel)
3. Log in using your username and password (for designated artists only!)
4. Click "CREATE NEW PIECE"
5. Add a title
6. Ensure that the "Filter" assetbundle is selected
7. Click "Generate & upload asset bundle"
8. You should now see your filter in the "Drafts" section of the Filta iOS app
