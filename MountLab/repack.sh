pushd ../../../packages/ILRepack*/tools; d=$(pwd); popd
mono $d/ILRepack.exe /out:.. /ndebug /internalize /renameInternalized /parallel /verbose *.exe *.dll
