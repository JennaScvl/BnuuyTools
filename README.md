# BnuuyTools
A collection of tools for manipulating Skinned Meshes from within the Unity editor

Many of these instantiate meshes and materials. Make sure that before you use the MeshSaver to save a copy of the instanced mesh to an asset file once you've finished manipulating your skinned mesh. Otherwise the mesh won't save to prefabs, and it won't upload properly to VRChat.

An Explanation of the tools.
Bone Merger: This merges a bone into its parent. This basically makes bones fuse into their parents. At currently it only works on bones with a parent but no children. Do not use it on your root bone nor on your armature, and only use it on bones.
Make sure the bone you're merging has no children. So like say you have hair that goes 3 children deep. First use it on the bottom most child, then its parent, then its parent, etc.
For example I have an avatar has has multiple physics bangs on its hair. They're like Bangs11.R / Bangs12.R / Bangs13.R. I use it on 13 first, then 12, then 11.
It applies the bone deletion to all relevant skinned meshes in the root of the object the armature you're deleting from is in.

Mesh Saver: This saves a SkinnedMeshRenderer's shared mesh as an asset and then applies that asset to the same SkinnedMeshRenderer. The idea here is to save your work.

Mesh Combiner: This script takes two SkinnedMeshRenderers and merges them into a single one. It leaves the originals just in case something didn't work right. The new one ends up being named after both of the ones it derived from. It usually works but sometimes it fails to work correctly. It attempts to preserve blendshapes, bones, weights, and even tries to take into account that the two meshes in question might reference bones the other mesh doesn't have. This lets you easily reduce the number of SkinnedMeshRenderers in the avatar.

Submesh Combiner: This goes through a SkinnedMeshRenderer's sharedmesh and collapses all submeshes that share the same material in order to reduce the number of material references in an avatar, since many booth avatars have redundant meshes. If you use Mesh Combiner on two meshes with shared materials and then use Submesh Combiner on the resulting mesh, it usually works very well together to further optimize the avatar.
