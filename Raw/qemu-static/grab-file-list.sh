cd /opt/w3top
rm -f /tmp/w3top-dependencies-raw.txt
for f in Universe.W3Top *.so; do
  ldd $f >> /tmp/w3top-dependencies-raw.txt
done
cat /tmp/w3top-dependencies-raw.txt | awk '{print $3}' | sort -u | tee /tmp/w3top-dependencies.txt
