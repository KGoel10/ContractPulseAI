
import { useState } from "react";
import DownloadIcon from "@mui/icons-material/Download";
import { Button, Grid, TextField, Typography } from "@mui/material";

// Vision UI Dashboard React components
import VuiBox from "components/VuiBox";

// Vision UI Dashboard React example components
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";

const inputSx = {
  "& .MuiInputBase-root": {
    width: "100%",
    boxSizing: "border-box",
    fontSize: "0.875rem",
    color: "#ffffff",
    backgroundColor: "rgba(6, 11, 40, 0.28) !important",
    backdropFilter: "blur(14px)",
    borderRadius: "14px",
    boxShadow: "inset 0 1px 0 rgba(255, 255, 255, 0.08)",
    transition: "background-color 180ms ease, box-shadow 180ms ease",
    "& fieldset": {
      borderColor: "rgba(160, 174, 192, 0.35)",
      transition: "border-color 180ms ease",
    },
    "&:hover": {
      backgroundColor: "rgba(6, 11, 40, 0.42) !important",
    },
    "&:hover fieldset": {
      borderColor: "rgba(57, 147, 254, 0.75)",
    },
    "&.Mui-focused fieldset": {
      borderColor: "#3993fe",
    },
    "&.Mui-focused": {
      backgroundColor: "rgba(6, 11, 40, 0.48) !important",
      boxShadow: "0 0 0 3px rgba(57, 147, 254, 0.14)",
    },
  },
  "& .MuiInputBase-input": {
    width: "100%",
    minWidth: 0,
    boxSizing: "border-box",
    fontSize: "0.875rem",
    color: "#ffffff !important",
    "&::selection": {
      backgroundColor: "rgba(57, 147, 254, 0.45)",
    },
  },
  "& .MuiInputBase-multiline": {
    display: "flex",
    alignItems: "stretch",
    width: "100%",
  },
  "& .MuiInputBase-inputMultiline": {
    display: "block",
    flex: "1 1 auto",
    width: "100%",
    minWidth: 0,
    boxSizing: "border-box",
    whiteSpace: "pre-wrap",
    overflowWrap: "break-word",
  },
  "& input::placeholder, & textarea::placeholder": {
    color: "rgba(255, 255, 255, 0.75)",
    opacity: 1,
  },
};

function SOW() {
  const [formValues, setFormValues] = useState({
    budget: "",
    duration: "",
    resource: "",
  });
  const details = localStorage.getItem("rfpPrompt") || "No details available.";

  const handleChange = (field) => (event) => {
    setFormValues((currentValues) => ({
      ...currentValues,
      [field]: event.target.value,
    }));
  };

  const handleDownload = () => {
    const lines = [
      "Statement of Work",
      "",
      `Budget: ${formValues.budget}`,
      `Duration: ${formValues.duration}`,
      `Resource: ${formValues.resource}`,
      "",
      ...details.split("\n"),
    ].flatMap((line) => {
      const words = line.split(" ");
      const wrappedLines = [];
      let currentLine = "";

      words.forEach((word) => {
        if (`${currentLine} ${word}`.trim().length > 88) {
          wrappedLines.push(currentLine);
          currentLine = word;
        } else {
          currentLine = `${currentLine} ${word}`.trim();
        }
      });

      wrappedLines.push(currentLine);
      return wrappedLines;
    });
    const escapePdfText = (value) =>
      value.replace(/\\/g, "\\\\").replace(/\(/g, "\\(").replace(/\)/g, "\\)");
    const textCommands = lines
      .map((line, index) => `1 0 0 1 54 ${760 - index * 16} Tm (${escapePdfText(line)}) Tj`)
      .join("\n");
    const pdfContent = `BT
/F1 12 Tf
${textCommands}
ET`;
    const objects = [
      "<< /Type /Catalog /Pages 2 0 R >>",
      "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
      "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
      "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
      `<< /Length ${pdfContent.length} >>\nstream\n${pdfContent}\nendstream`,
    ];
    let pdf = "%PDF-1.4\n";
    const offsets = [0];

    objects.forEach((object, index) => {
      offsets.push(pdf.length);
      pdf += `${index + 1} 0 obj\n${object}\nendobj\n`;
    });

    const xrefOffset = pdf.length;
    pdf += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n`;
    offsets.slice(1).forEach((offset) => {
      pdf += `${String(offset).padStart(10, "0")} 00000 n \n`;
    });
    pdf += `trailer\n<< /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xrefOffset}\n%%EOF`;

    const file = new Blob([pdf], { type: "application/pdf" });
    const downloadUrl = URL.createObjectURL(file);
    const link = document.createElement("a");

    link.href = downloadUrl;
    link.download = "statement-of-work.pdf";
    link.click();
    URL.revokeObjectURL(downloadUrl);
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <VuiBox mt={4}>
        <VuiBox my={3}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Typography
                variant="h5"
                sx={{ color: "#d8eaff", fontSize: "1.35rem", fontWeight: 500 }}
                gutterBottom
              >
                Statement of Work
              </Typography>
              <Typography variant="body2" sx={{ color: "rgba(181, 211, 244, 0.78)", fontSize: "0.85rem" }} mb={3}>
                Define the project details and download your statement of work.
              </Typography>

              <Grid container spacing={2} alignItems="center">
                <Grid item xs={12} sm={3}>
                  <Typography variant="body1" sx={{ color: "#a9cdf2", fontSize: "0.875rem" }}>
                    Budget
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={9}>
                  <TextField
                    fullWidth
                    value={formValues.budget}
                    onChange={handleChange("budget")}
                    placeholder="Enter project budget"
                    sx={inputSx}
                  />
                </Grid>

                <Grid item xs={12} sm={3}>
                  <Typography variant="body1" sx={{ color: "#a9cdf2", fontSize: "0.875rem" }}>
                    Duration
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={9}>
                  <TextField
                    fullWidth
                    value={formValues.duration}
                    onChange={handleChange("duration")}
                    placeholder="Enter project duration"
                    sx={inputSx}
                  />
                </Grid>

                <Grid item xs={12} sm={3}>
                  <Typography variant="body1" sx={{ color: "#a9cdf2", fontSize: "0.875rem" }}>
                    Resource
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={9}>
                  <TextField
                    fullWidth
                    value={formValues.resource}
                    onChange={handleChange("resource")}
                    placeholder="Enter required resources"
                    sx={inputSx}
                  />
                </Grid>

                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ color: "#b9d9fa", fontSize: "0.95rem", fontWeight: 500, mb: 1 }}>
                    Generated SOW
                  </Typography>
                  <VuiBox
                    minHeight="200px"
                    p={2}
                    sx={{
                      color: "#e7f1ff",
                      backgroundColor: "rgba(17, 43, 78, 0.34)",
                      backdropFilter: "blur(14px)",
                      border: "1px solid rgba(91, 162, 231, 0.32)",
                      borderRadius: "14px",
                      whiteSpace: "pre-wrap",
                      overflowWrap: "break-word",
                    }}
                  >
                    {details}
                  </VuiBox>
                </Grid>
              </Grid>

              <VuiBox display="flex" justifyContent="flex-end" mt={3}>
                <Button
                  size="small"
                  variant="contained"
                  color="info"
                  onClick={handleDownload}
                  startIcon={<DownloadIcon />}
                  sx={{
                    px: 2.5,
                    color: "#0b0a0a",
                    "&.Mui-disabled": {
                      color: "rgba(255, 255, 255, 0.6)",
                    },
                  }}
                >
                  Download
                </Button>
              </VuiBox>
            </Grid>
          </Grid>
        </VuiBox>
      </VuiBox>
    </DashboardLayout>
  );
}

export default SOW;
