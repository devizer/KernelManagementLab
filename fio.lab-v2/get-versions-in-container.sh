export ldd_version="$(ldd --version | head -1 |  awk '{print $NF}')"
export libaio_version="$(apt-cache policy libaio-dev | grep andidate | awk '{print $NF}')"
export fio_version="$(apt-cache policy fio | grep andidate | awk '{print $NF}')"
if [[ "$(command -v yum)" != "" ]]; then
  export libaio_version="$(yum info libaio-devel | grep Version | awk '{print $NF}')"
  export fio_version="$(yum info fio | grep Version | awk '{print $NF}')"
fi
