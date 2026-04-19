from distutils.version import StrictVersion
try:
    v = StrictVersion("1.5.0")
    print(f"StrictVersion('1.5.0').version: {getattr(v, 'version', 'MISSING')}")
    print(f"Type of v: {type(v)}")
except Exception as e:
    print(f"Error: {e}")
