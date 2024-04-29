import requests
import urllib.parse
import re

# Check for variable if it is hash 
def is_valid_hash(variable): 
    pattern = r'\$2y\$10\$[A-Za-z0-9./]{53}'
    match = re.match(pattern, variable)
    return match is not None

# Get all available paths from URL. Note that intial path could be '/' or anything defined on server side. Additionally add different paths for different types of APIs.
def get_available_paths(url):
    response = requests.get(url)
    data = response.json()
    return data["paths"]

# First test of Get Methods. Note that this should be called even after testing all POST methods to check if they worked successfully. 
def testGetMethods(paths, url):
    for path in paths:
        if 'GET' in path['methods'] and path['path'] != '/': # Currently only supports depth of 1, additional logic for depth crawling needed
            if '{' in path['path']:
                path_value = path['path']
                split_path = path_value.split('/')
                base_path = '/' + split_path[1]
                response = requests.get(url + base_path)
                for respLine in response.json():
                    for i in range(len(respLine)):
                        variable = respLine[i]
                        if isinstance(variable, str) and is_valid_hash(variable): # Skip if HASH
                            print(f"Skipping hash {variable}")
                            print()
                        else:
                            response = requests.get(url + base_path + '/' + urllib.parse.quote(str(variable)))
                            if "detail" not in response:
                                print(f"Path: {base_path + '/' + str(variable)}")
                                print(f"Response: {response.text}")
                                print()
            else:
                response = requests.get(url + path['path'])
                print(f"Path: {path['path']}")
                print(f"Response: {response.text}")
                print()

# TODO: Test all Post methods, will need all different types based on lengths 
def testPostMethods():
    return

# TODO: Tets all Delete methods
def testDeleteMethods():
    return