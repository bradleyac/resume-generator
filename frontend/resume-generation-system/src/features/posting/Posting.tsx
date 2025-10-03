import { useParams } from "react-router-dom";
import styles from "./Posting.module.css";
import { useEffect, useState } from "react";

const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;
type PostingBody = { id: string, postingText: string, importedAt: string, resumeUrl: string }

export const Posting = () => {
  const { postingId } = useParams();

  const [posting, setPosting] = useState<PostingBody | undefined>(undefined);

  useEffect(() => {
    let ignore = false;

    const fetchItems = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/GetCompletedPosting?completedPostingId=${postingId}`,);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        if (!ignore) setPosting(data);
      } catch (error) {
        console.log(error);
      }
    };

    fetchItems();

    return () => { ignore = true };
  }, [postingId]);

  if (!posting) {
    return <div>Loading...</div>;
  }

  return (<article className={styles.posting}>
    <label htmlFor="posting-id">Posting Id: </label>
    <p id="posting-id">{posting.id}</p>
    <label htmlFor="artifacts">Artifacts: </label>
    <div id="artifacts" className={styles.artifacts}>
      <a href={posting.resumeUrl}>Resume</a>
    </div>
    <label htmlFor="imported">Imported: </label>
    <p id="imported">{new Date(posting.importedAt).toLocaleString()}</p>
    <label htmlFor="job-posting">Job Posting: </label>
    <p id="job-posting" className={styles.postingText}>{posting.postingText}</p>
  </article>);
}