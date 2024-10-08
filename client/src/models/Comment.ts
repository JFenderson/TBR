import { Video } from './Video';
import Recruiter  from './Recruiter';
import  Student  from './Student';

export default interface Comment {
    commentId: string;
    content: string;
    commentDate: string;
    studentId: string; // Reference to the student whose video is commented on
    videoId?: string; // Reference to the video being commented on
    recruiterId: string; // Reference to the recruiter making the comment
    video?: Video;
    recruiter?: Recruiter;
    student?: Student;
}
