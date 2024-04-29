import spider

paths = spider.get_available_paths('http://192.168.8.107:8000/') # Replace with the actual URL
spider.testGetMethods(paths,'http://192.168.8.107:8000')