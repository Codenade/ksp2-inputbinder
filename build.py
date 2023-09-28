import os, subprocess, os.path, shutil, argparse

class f_styles:
    STEP_SUCCESS = '\033[96m'
    SUCCESS = '\033[92m'
    FAIL = '\033[91m'
    NORMAL = '\033[0m'
    INFO = '\033[95m'

def execute_build():
  parser = argparse.ArgumentParser()
  parser.add_argument("-e", "--unity-executable", required=False, type=str, default="C:/Program Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe", help="If your unity installation is not located at \"C:/Program Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe\" use this option")
  parser.add_argument("-i", "--install", required=False, default=False, action="store_true", help="Install this mod")
  parser.add_argument("-s", "--start", required=False, default=False, action="store_true", help="Install and start this mod, --install is redundant when using this option")
  parser.add_argument("-d", "--debug", required=False, default=False, action="store_true", help="Produces a debug build with full debug information")
  parser.add_argument("-n", "--no-archive", required=False, default=False, action="store_true", help="Do not create an archive file when completed")
  parser.add_argument("--skip-assembly-build", required=False, default=False, action="store_true", help="Skips the assembly build")
  parser.add_argument("--skip-assets", required=False, default=False, action="store_true", help="Skips the addressables build")
  args = parser.parse_args()
  shutil.rmtree(os.path.join(os.getcwd(), "build"), True)
  if not args.skip_assembly_build:
    print("Starting assembly build")
    try:
      if args.debug:
        print(f_styles.INFO + "Selected debug build" + f_styles.NORMAL)
        subprocess.run("dotnet build -c \"Debug\"", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
      else:
        subprocess.run("dotnet build -c \"Release\"", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
      print(f_styles.STEP_SUCCESS + "Assembly build finished" + f_styles.NORMAL)
    except:
      error("Assembly build failed")
  else:
    print(f_styles.INFO + "Skipped assembly build" + f_styles.NORMAL)
  if not args.skip_assets:
    print("Building assets")
    try:
      subprocess.run(executable=args.unity_executable, args="-projectPath \"" + os.getcwd() + "/ksp2-inputbinder-assets/\" -quit -batchmode -executeMethod BuildAssets.PerformBuild", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
      shutil.copytree("ksp2-inputbinder-assets/Library/com.unity.addressables/aa/windows", "build/BepInEx/plugins/inputbinder/addressables", dirs_exist_ok=True)
      print(f_styles.STEP_SUCCESS + "Building assets finished" + f_styles.NORMAL)
    except Exception as e:
      error("Building assets failed: " + e.__str__())
  else:
    print(f_styles.INFO + "Skipped building assets" + f_styles.NORMAL)
  print("Copying README.md and LICENSE.txt")
  shutil.copy("README.md", "build/BepInEx/plugins/inputbinder/README.md")
  shutil.copy("LICENSE.txt", "build/BepInEx/plugins/inputbinder/LICENSE.txt")
  if not args.no_archive:
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