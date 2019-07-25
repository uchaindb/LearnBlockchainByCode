
cd ./tmp

chapters="\
	README.md \
	Prologue.md \

	section1/README.md \
	section1/1-Index.md \
	section1/1-History.md \
	section1/1-Overview.md \
	section1/1-Basics.md \

	section1/2-Index.md \
	section1/2-Structure.md \
	section1/2-Block.md \
	section1/2-Accounting.md \
	section1/2-Creating.md \
	section1/2-Exercise.md \

	section1/3-Index.md \
	section1/3-Signature.md \
	section1/3-Hash-Tree.md \
	section1/3-SPV.md \
	section1/3-Transaction.md \
	section1/3-Light-Client.md \
	section1/3-Exercise.md \

	section1/4-Index.md \
	section1/4-P2P.md \
	section1/4-Management.md \
	section1/4-Broadcast.md \
	section1/4-Consensus.md \
	section1/4-Exercise.md \

	section1/5-Index.md \
	section1/5-Lock-Time.md \
	section1/5-Execution-Environment.md \
	section1/5-Smart-Contracts.md \
	section1/5-Exercise.md \
	section1/6-Index.md \
	"

options="../metadata.yaml --highlight-style tango"

# pandoc -o full-speed-python.epub $options $chapters
# pandoc -o full-speed-python.pdf -H tex/preamble.tex $options --latex-engine=xelatex $chapters
pandoc -o ../chainbook.pdf -H ../tex/preamble.tex $options $chapters

