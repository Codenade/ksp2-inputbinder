import os, subprocess, os.path, shutil, argparse

class h_styles:
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    RED = '\033[91m'
    ENDCOLOR = '\033[0m'

def execute_build():
  parser = argparse.ArgumentParser()
  parser.add_argument("--unity_executable", required=False, type=str, default="C:/Program Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe")
  args = parser.parse_args()
  shutil.rmtree(os.path.join(os.getcwd(), "build"), True)
  print("Starting assembly build")
  try:
    subprocess.run("dotnet build", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT)
  except:
    error()
  print(h_styles.CYAN + "Assembly build finished" + h_styles.ENDCOLOR)
  print("Building assets")
  try:
    subprocess.run(executable=args.unity_executable, args="-projectPath \"" + os.getcwd() + "/ksp2-inputbinder-assets/\" -quit -batchmode -executeMethod BuildAssets.PerformBuild", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT)
    shutil.copytree("ksp2-inputbinder-assets/Library/com.unity.addressables/aa/windows", "build/BepInEx/plugins/inputbinder/addressables", dirs_exist_ok=True)
  except Exception as e:
    print(h_styles.RED + e.__str__() + h_styles.ENDCOLOR)
    error()
  print(h_styles.CYAN + "Building assets finished" + h_styles.ENDCOLOR)
  print("Creating build.zip")
  try:
    shutil.make_archive("build/build", "zip", "build", "BepInEx")
  except:
    error()
  print(h_styles.GREEN + "Done" + h_styles.ENDCOLOR)
  exit(0)

def error():
  print(h_styles.RED + "Build failed" + h_styles.ENDCOLOR)
  exit(1)

if __name__ == "__main__":
  execute_build()