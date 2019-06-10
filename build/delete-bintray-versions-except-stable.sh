set +e
set -u

API=https://api.bintray.com
BINTRAY_USER=devizer
BINTRAY_REPO=W3-Top
PCK_NAME=W3Top

base_url="curl -u${BINTRAY_USER}:${BINTRAY_API_KEY} -H Content-Type:application/json -H Accept:application/json"

function delete_version() {
  v=$1
  url="$base_url -X DELETE ${API}/packages/${BINTRAY_USER}/${BINTRAY_REPO}/${PCK_NAME}/versions/$v"
  eval $url
  echo ""
}


url="$base_url -X GET ${API}/packages/${BINTRAY_USER}/${BINTRAY_REPO}/${PCK_NAME}"
jsonPackage=`eval $url`
printf "\n\nPACKAGE\n${jsonPackage}\n"
jsonVersions=`echo $jsonPackage | jq -r '.versions'`
printf "\n\nVERSIONS as json\n${jsonVersions}\n"

jsonVersions2=`echo $jsonPackage | jq -r '.versions[]'`
printf "\n\nVERSIONS as bash array\n${jsonVersions2}\n"

for ver in $jsonVersions2; do
  echo checking version $ver ...
  if [[ "${ver}" != "${VERSION_STABLE}" ]]; then
    echo "delete version $ver"
    delete_version "$ver"
  else
    echo Keep version $ver
  fi
  echo ""
done

