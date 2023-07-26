import os, subprocess, os.path, shutil, argparse

class f_styles:
    STEP_SUCCESS = '\033[96m'
    SUCCESS = '\033[92m'
    FAIL = '\033[91m'
    NORMAL = '\033[0m'

def execute_build():
  parser = argparse.ArgumentParser()
  parser.add_argument("--unity_executable", required=False, type=str, default="C:/Program Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe")
  parser.add_argument("--install", required=False, default=False, action="store_true")
  parser.add_argument("--start", required=False, default=False, action="store_true")
  args = parser.parse_args()
  shutil.rmtree(os.path.join(os.getcwd(), "build"), True)
  print("Starting assembly build")
  try:
    subprocess.run("dotnet build", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
    print(f_styles.STEP_SUCCESS + "Assembly build finished" + f_styles.NORMAL)
  except:
    error("Assembly build failed")
  print("Building assets")
  try:
    subprocess.run(executable=args.unity_executable, args="-projectPath \"" + os.getcwd() + "/ksp2-inputbinder-assets/\" -quit -batchmode -executeMethod BuildAssets.PerformBuild", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
    shutil.copytree("ksp2-inputbinder-assets/Library/com.unity.addressables/aa/windows", "build/BepInEx/plugins/inputbinder/addressables", dirs_exist_ok=True)
    print(f_styles.STEP_SUCCESS + "Building assets finished" + f_styles.NORMAL)
  except Exception as e:
    error("Building assets failed: " + e.__str__())
  print("Copying README.md and LICENSE.txt")
  shutil.copy("README.md", "build/BepInEx/plugins/inputbinder/README.md")
  shutil.copy("LICENSE.txt", "build/BepInEx/plugins/inputbinder/LICENSE.txt")
  print("Creating build.zip")
  try:
    shutil.make_archive("build/build", "zip", "build", "BepInEx")
  except:
    error("Could not create build.zip")
  print(f_styles.SUCCESS + "SUCCESS: Build finished" + f_styles.NORMAL)
  if args.install or args.start:
    print("Installing to \'" + os.getenv("KSP2_PATH") + "\'")
    try:
      shutil.copytree(src="build", dst=os.getenv("KSP2_PATH"), ignore=shutil.ignore_patterns("*.zip"), dirs_exist_ok=True)
    except:
      error("Failed to install")
  if args.start:
    print("Starting KSP2")
    os.system("\"" + shutil.which(os.path.join(os.getenv("KSP2_PATH"), "KSP2_x64.exe")) + "\"")
  exit(0)

def error(msg: str):
  print(f_styles.FAIL + "FAILED: " + msg + f_styles.NORMAL)
  exit(1)

if __name__ == "__main__":
  execute_build()