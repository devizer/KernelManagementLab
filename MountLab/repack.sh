pushd ../../../packages/ILRepack*/tools; d=$(pwd); popd
mono $d/ILRepack.exe /out:../MountLab.exe /ndebug /parallel *.exe *.dll
