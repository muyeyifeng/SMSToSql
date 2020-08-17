rm -rf hash
files=$(ls)
for filename in $files
do
	sha1sum $filename >> 'hash'
done
