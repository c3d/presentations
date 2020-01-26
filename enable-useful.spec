run_configure_disable_everything                \
    --target-list="%{user_mini_arch}-softmmu"   \
    --enable-kvm                                \
    --enable-system                             \
    --enable-virtfs                             \
    --enable-cap-ng                             \
    --enable-attr                               \
    --enable-vhost-vsock                        \
    --enable-vhost-kernel                       \
    --enable-vhost-net                          \
    --enable-vhost-scsi                         \
    --enable-linux-aio                          \
    --enable-debug-info                         \
    --enable-virtio-mini                        \
    %{user_mini_fdt}
