#!/usr/bin/env bash
command -v dpl || (sudo apt-get update -yq; sudo apt-get install -y ruby-dev; sudo gem install dpl dpl-releases --pre)

set -e
set -u

pushd build >/dev/null
# ./inject-git-info.ps1
verFile=./AppGitInfo.json
ver=$(cat $verFile | jq -r ".Version")
popd >/dev/null

# https://github.com/travis-ci/dpl#github-releases
# https://github.com/schibsted/travis-dpl#github-releases

repo_name=$(basename `git rev-parse --show-toplevel`)
commit_id=$(git rev-parse HEAD)
echo "version: [${ver}], repo: [$repo_name], commit: [${commit_id}]"

git reset --hard
git tag -f "v${ver}"
git push --tags


function new_dpl() {
  sudo dpl releases \
    --token $GITHUB_RELEASE_TOKEN \
    --repo devizer/$repo_name \
    --file "*.md" \
    --overwrite \
    --prerelease \
    --release_number "v${ver}" \
    --release_notes "The v${ver} Release" \
    --tag_name "v${ver}" 
    
#    --file ./Universe.W3Top/bin/w3top*.tar.gz* \
}


function old_dpl() {
    # sudo apt-get install -y ruby-dev; sudo gem install dpl dpl-releases
    for files in "./Universe.W3Top/bin/w3top*.tar.gz*" "WHATSNEW.md"; do
      echo "KEY: $GITHUB_RELEASE_TOKEN"
      dpl --provider=releases --api-key=$GITHUB_RELEASE_TOKEN \
        --name="W3Top Stable ${ver}" \
        --body="It is not supposed to direct downloading files here. Please take a look on installation options on https://github.com/devizer/w3top-bin#reinstallation-of-precompiled-binaries" \
        --file="$files" \
        --skip-cleanup \
        --repo=devizer/$repo_name
    done
}

old_dpl
# $GITHUB_RELEASE_TOKEN
# --file-glob=true \ --overwrite=true \
