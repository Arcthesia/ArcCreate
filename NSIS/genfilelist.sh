#!/bin/sh
# generate file list
nsisdir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)

pwd
ls

# ensure Unix ending
sed -i 's/\r//g' userfiles
sed -i 's/\r//g' excludes
# generate install list
cd ./StandaloneWindows64/
find . -type d -printf '%P\n' | sed 's/\//\\/g;s/\.\\//' | sort -r | grep -vxFf ${nsisdir}/userfiles | grep -vxFf ${nsisdir}/excludes | xargs -r -d '\n' printf 'CreateDirectory "$InstDir\\%s"\r\n' > ${nsisdir}/install-files.nsi
find . -type f -printf '%P\n' | sed 's/\//\\/g;s/\.\\//' | grep -vxFf ${nsisdir}/userfiles | grep -vxFf ${nsisdir}/excludes | xargs -r -d '\n' printf '${_File} "%s"\r\n' >> ${nsisdir}/install-files.nsi
cat ${nsisdir}/userfiles | xargs -r -d '\n' printf '${_OptionalFile} "%s" "$InstDir"\r\n' > ${nsisdir}/install-files-optional.nsi

# generate unistall list
# optional files
cat ${nsisdir}/userfiles | xargs -r -d '\n' printf 'Delete "$InstDir\\%s"\r\n' > ${nsisdir}/uninstall-files-optional.nsi
cat ${nsisdir}/userfiles | xargs -r -d '\n' printf 'Delete "$InstDir\\%s.new"\r\n' >> ${nsisdir}/uninstall-files-optional.nsi
# main app
find . -type f -printf '%P\n' | sed 's/\//\\/g;s/\.\\//' | grep -vxFf ${nsisdir}/userfiles | grep -vxFf ${nsisdir}/excludes | xargs -r -d '\n' printf 'Delete "$InstDir\\%s"\r\n' > ${nsisdir}/uninstall-files.nsi
# directory
find . -type d -printf '%P\n' | sed 's/\//\\/g;s/\.\\//' | sort -r | grep -vxFf ${nsisdir}/userfiles | grep -vxFf ${nsisdir}/excludes | xargs -r -d '\n' printf 'RMDir "$InstDir\\%s"\r\n' >> ${nsisdir}/uninstall-files.nsi

cat ${nsisdir}/install-files.nsi
cat ${nsisdir}/install-files-optional.nsi
cat ${nsisdir}/uninstall-files.nsi
cat ${nsisdir}/uninstall-files-optional.nsi