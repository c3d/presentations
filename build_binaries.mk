# Not using gobuild here in order to stick to how upstream builds
# (This builds multiple binaries)
%build
export PATH=$PATH:"$(pwd)/go/bin"
export GOPATH="$(pwd)/go"

mkdir -p go/src/%{domain}/%{org}
ln -s $(pwd)/../%{repo}-%{version} go/src/%{importname}
cd go/src/%{importname}
make \
    QEMUPATH=%{_bindir}/%{qemu} \
    SKIP_GO_VERSION_CHECK=y \
    MACHINETYPE="q35" \
    KERNELPARAMS="systemd.unified_cgroup_hierarchy=0"
