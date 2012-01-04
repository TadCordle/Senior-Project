using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace Senior_Project
{
	partial class frmAIChooser : Form
	{
		private Type ai1Type, ai2Type;

		public Type AI1Type
		{
			get { return ai1Type; }
			set
			{
				if (!value.IsSubclassOf(typeof(AI)))
					throw new ArgumentException("AI1 does not subclass AI!");

				if (!cmbAI1.Items.Contains(value))
					cmbAI1.Items.Add(value);

				ai1Type = value;
				cmbAI1.SelectedItem = value;
			}
		}
		public Type AI2Type
		{
			get { return ai2Type; }
			set
			{
				if (!value.IsSubclassOf(typeof(AI)))
					throw new ArgumentException("AI2 does not subclass AI!");

				if (!cmbAI2.Items.Contains(value))
					cmbAI2.Items.Add(value);

				ai2Type = value;
				cmbAI2.SelectedItem = value;
			}
		}

		public frmAIChooser()
		{
			InitializeComponent();

			var asm = Assembly.GetExecutingAssembly();
			var aitypes = new List<Type>();
			foreach (Type t in asm.GetTypes())
				if (t.IsSubclassOf(typeof(AI)))
					aitypes.Add(t);

			this.cmbAI1.Items.AddRange(aitypes.ToArray());
			this.cmbAI2.Items.AddRange(aitypes.ToArray());
		}

		#region Event Handlers
		private void btnBrowseAI1_Click(object sender, EventArgs e)
		{
			var result = this.ofd.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				var aitypes = compileAI(this.ofd.FileName);
				if (aitypes == null)
					return;

				MessageBox.Show(string.Format("Loaded {0} AI class{1} from {2}",
					aitypes.Length, aitypes.Length == 1 ? "" : "s", this.ofd.FileName),
					"Dynamic AI Loading",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				this.cmbAI1.Items.AddRange(aitypes);
			}
		}
		private void btnBrowseAI2_Click(object sender, EventArgs e)
		{
			var result = this.ofd.ShowDialog();
			if (result == DialogResult.OK)
			{
				var aitypes = compileAI(this.ofd.FileName);
				if (aitypes == null)
					return;

				MessageBox.Show(string.Format("Loaded {0} AI class{1} from {2}",
					aitypes.Length, aitypes.Length == 1 ? "" : "s", this.ofd.FileName),
					"Dynamic AI Loading",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				this.cmbAI2.Items.AddRange(aitypes);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
		private void cmbAI1_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.ai1Type = cmbAI1.SelectedItem as Type;
		}
		private void cmbAI2_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.ai2Type = cmbAI2.SelectedItem as Type;
		}
		#endregion

		private static Type[] compileAI(string filename)
		{
			using (var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } }))
			{
				var param = new CompilerParameters(new[] { "mscorlib.dll", "System.dll", "System.Core.dll",
				Assembly.GetExecutingAssembly().Location });
				param.GenerateExecutable = false;
				param.GenerateInMemory = true;
				param.Evidence = new Evidence(new[] { new Zone(SecurityZone.Internet) }, null);

				// Forcing the compiled assembly to be security transparent results in it being unable
				// to demand any access to the filesystem, so malicious AI code cannot access filesystem, etc.
				string src = "[assembly:System.Security.SecurityTransparent]\n" + File.ReadAllText(filename);

				var results = csc.CompileAssemblyFromSource(param, src);
				var compilererrors = results.Errors.Cast<CompilerError>();
				var warnings = compilererrors.Where(x => x.IsWarning).ToArray();
				var errors = compilererrors.Where(x => !x.IsWarning).ToArray();

				if (results.Errors.HasErrors || results.Errors.HasWarnings)
				{
					var head = "Code compilation returned {0}{1}{2}:\n\n{3}";
					var warntext = results.Errors.HasWarnings ? string.Format("{0} warning{1}", warnings.Length, warnings.Length == 1 ? "" : "s") : "";
					var errtext = results.Errors.HasErrors ? string.Format("{0} error{1}", errors.Length, errors.Length == 1 ? "" : "s") : "";
					var joint = results.Errors.HasErrors && results.Errors.HasWarnings ? " and " : "";
					var details = string.Join("\n", compilererrors.Select(x => x.ToString()).ToArray());

					MessageBox.Show(string.Format(head, errtext, joint, warntext, details),
						"Compiler Results",
						MessageBoxButtons.OK,
						results.Errors.HasErrors ? MessageBoxIcon.Error : MessageBoxIcon.Warning);

					if (results.Errors.HasErrors)
						return null;
				}

				return (from x in results.CompiledAssembly.GetTypes()
						where x.IsSubclassOf(typeof(AI))
						select x).ToArray();
			}
		}
	}
}
