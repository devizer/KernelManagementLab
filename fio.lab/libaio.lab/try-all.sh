# work=/transient-builds/fio; sudo mkdir -p $work; sudo chown -R $(whoami) $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd fio.lab/libaio.lab; bash try-all.sh
cat ver-links.txt | while read -r ver link; do
    if [[ -z "$ver" ]]; then continue; fi
    echo "libaio $ver: $link"
    work=/transient-builds/libaio-src/$ver
    mkdir -p $work
    rm -rf $work/*
    pushd $work
    wget -O $ver $link
    tar xzf $ver
    rm -f $ver
    cd lib*
    Say "VER $ver"
    time make prefix=/transient-builds/libaio-dev/$ver install
    popd
done 
