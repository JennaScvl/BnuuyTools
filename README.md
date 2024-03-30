# BnuuyTools<br>
A collection of tools for manipulating Skinned Meshes from within the Unity editor<br>
<br>
Many of these instantiate meshes and materials. Make sure that before you use the MeshSaver to save a copy of the instanced mesh to an asset file once you've finished manipulating your skinned mesh. Otherwise the mesh won't save to prefabs, and it won't upload properly to VRChat.<br>
<br>
An Explanation of the tools.<br>
<br>
# Mesh Saver
This saves a SkinnedMeshRenderer's shared mesh as an asset and then applies that asset to the same SkinnedMeshRenderer. The idea here is to save your work. Remember, all of these scripts work with instantiated meshes rather than overwriting the original asset, so you need to save your work as you go along.<br>
<br>
# Bone Merger
This merges a bone into its parent. This basically makes bones fuse into their parents. At currently it only works on bones with a parent but no children. Do not use it on your root bone nor on your armature, and only use it on bones.<br>
Make sure the bone you're merging has no children. So like say you have hair that goes 3 children deep. First use it on the bottom most child, then its parent, then its parent, etc.<br>
<br>
For example I have an avatar has has multiple physics bangs on its hair. They're like Bangs11.R / Bangs12.R / Bangs13.R. I use it on 13 first, then 12, then 11.<br>
<br>
It applies the bone deletion to all relevant skinned meshes in the root of the object the armature you're deleting from is in, but they all have to be enabled first.<br>
<br>
<b>Requires Mesh Saver when you're done.<b><br>
<br>
# Mesh Combiner
This script takes two SkinnedMeshRenderers and merges them into a single one. It leaves the originals just in case something didn't work right. The new one ends up being named after both of the ones it derived from. It usually works but sometimes it fails to work correctly. It attempts to preserve blendshapes, bones, weights, and even tries to take into account that the two meshes in question might reference bones the other mesh doesn't have. This lets you easily reduce the number of SkinnedMeshRenderers in the avatar.<br>
<br>
Recent update. It's been made more robust and now preserves blendshapes and orietations better. Still not perfect but getting there.
<br>
<b>Requires Mesh Saver when you're done.<b><br>
<br>
# Submesh Combiner
This goes through a SkinnedMeshRenderer's sharedmesh and collapses all submeshes that share the same material in order to reduce the number of material references in an avatar, since many booth avatars have redundant meshes. If you use Mesh Combiner on two meshes with shared materials and then use Submesh Combiner on the resulting mesh, it usually works very well together to further optimize the avatar.<br>
<br>
<b>Requires Mesh Saver when you're done.</b><br>
<br>
# Bone Retargeting
This lets take something skinned from one avatar and put it on another. The mesh field is the SkinnedMeshRenderer for the mesh you want to merge into an avatar. The Target Amarture is the armature of the actual avatar you're going to be putting it in. Beware, the TargetArmature needs to have all relevant bones present and they must have the correct names. You do not need to use Mesh Saver after using this.<br>
<br>
<b>Does not require Mesh Saver when you're done.</b><br>
# Skinned Submesh Deleter
This will list all of the submeshes on a skinned mesh by the name of the material they're using. You can then hit the delete button to delete the triangles in that submesh.<br>
It does not delete the vertices to make it easier to leave blendshapes and weights alone. However it also doesn't get rid of the submesh itself. To do that, I recommend using the Submesh Combiner. Just set the materials on the deleted submeshes to an existing material and hit combine. To that end make sure you have the latest version of Submesh Combiner because I had to update it to support merging in the resulting 0 triangle submeshes. It's also been made more robust as it preserves blend shapes and orientation better now.<br>
<br>
<b>Requires Mesh Saver when you're done.</b><br>
# UV Baker
This one's a little roundabout. What you do is you make an atlased texture and use that texture in all of the materials you want to atlas. Then adjust the repeats and offsets for each one so that they're compensating for the fact that the texture's been replaced with an atlas. Once you have that done, go to the skinned mesh renderer on the object and click "Bake UVs" at the bottom. Pretty much instantly it should look messed up again. Don't worry, just make one more material that uses the atlas with the repeats at 1,1 and the offsets at 0,0. Now you can use that material on each of the submeshes that you're making the atlas for. Once you have that done, use the Submesh combiner to combin those submeshes down to a single submesh and thus a single material instance.
<br>
<b>Requires Mesh Saver when you're done.</b><br>
# UV Unwrapper
This script takes any mesh (has to be the actual mesh, not the mesh renderer, so save the mesh to an asset with the Mesh Saver if you need to) and it will output textures in your assets folder. One texture per submesh, and it's shows the UVs for those meshes. It makes texture editing so much easier.
