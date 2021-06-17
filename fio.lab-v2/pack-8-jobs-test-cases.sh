rm -rf DUMPS
function pack() {
    src_dir_name=$1
    dst_dir_name=$2
    mkdir -p $dst_dir_name
    cur=$(pwd)
    find -type d -name $src_dir_name | while read d; do
      echo $d
      pushd $d
        cp -r * $cur/$dst_dir_name
      popd
    done
}

pack "dumps" "DUMPS/1-job"
pack "dumps-8-jobs" "DUMPS/8-jobs"
