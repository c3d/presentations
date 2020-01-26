%build
# Adjust for go build requirements
# Future: Use $gopkginstall
# export GOROOT="$(pwd)/go"
export PATH=$PATH:"$(pwd)/go/bin"
export GOPATH="$(pwd)/go"

mkdir -p go/src/%{domain}/%{org}
ln -s $(pwd)/../%{repo}-%{version} go/src/%{importname}
cd go/src/%{importname}
make \
    QEMUPATH=%{_bindir}/%{qemu} \
    not_check_version=certainly_not
