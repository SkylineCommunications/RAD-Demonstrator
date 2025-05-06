import argparse
import pathlib
import shutil

PROTOCOLS = ['AMS Server', 'SFP', 'Content Management Server 1','Content Management Server 2', 'Content Management Server 3']
OUTPUT_DIRECTORY = r'C:\GIT\CompanionFiles\Generic\Tutorial\Proactive Cap Detection Tutorial\Content\Protocols'

argparser = argparse.ArgumentParser(description='Create .dmprotocol files from directories with protocols')
argparser.add_argument('protocols', nargs='*', help='Directories with protocols')
argparser.add_argument('--output-dir', help='Output directory for .dmprotocol files')
argparser.add_argument('--package-name', help='Name of the package (can only be used with one protocol)')
args = argparser.parse_args()

if args.output_dir is None:
    args.output_dir = OUTPUT_DIRECTORY
if len(args.protocols) == 0:
    args.protocols = PROTOCOLS
if len(args.protocols) > 1 and args.package_name is not None:
    raise ValueError('Package name argument can only be used with one protocol')

output_dir = pathlib.Path(args.output_dir)
output_dir.mkdir(parents=True, exist_ok=True)
for protocol in args.protocols:
    if args.package_name is not None:
        package_name = args.package_name
    else:
        package_name = pathlib.Path(protocol).name
    
    print(f'Creating {package_name}.dmprotocol in {output_dir}')
    output_file = output_dir / f'{package_name}'
    shutil.make_archive(output_file, 'zip', protocol)
    shutil.move(f'{output_file}.zip', f'{output_file}.dmprotocol')